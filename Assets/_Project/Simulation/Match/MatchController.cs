using System.Collections.Generic;
using Fusion;
using MangaLegends.Core.Simulation;
using MangaLegends.Data.Definitions;
using MangaLegends.Simulation.Champions;
using MangaLegends.Simulation.Services;
using MangaLegends.Simulation.Units;
using UnityEngine;

namespace MangaLegends.Simulation.Match
{
    public enum MatchPhase : byte
    {
        Lobby,
        Playing,
        Ended
    }

    [RequireComponent(typeof(WaveDirector), typeof(JungleDirector))]
    public sealed class MatchController : NetworkBehaviour, IPlayerJoined, IPlayerLeft
    {
        private const int StructureCapacity = 32;
        private const float CorpseSeconds = 1.5f;
        public const int NoLane = 255;

        [SerializeField] private MapLayout _map;
        [SerializeField] private NetworkObject _playerSlotPrefab;

        private readonly List<PlayerSlot> _slots = new List<PlayerSlot>();
        private readonly List<(NetworkObject obj, int tick)> _corpses = new List<(NetworkObject, int)>();
        private MatchServices _services;
        private MatchRewards _rewards;

        [Networked] public MatchPhase Phase { get; set; }
        [Networked] public int StartTick { get; set; }
        [Networked] public int EndTick { get; set; }
        [Networked] public Team Winner { get; set; }
        [Networked] public ulong Seed { get; set; }
        [Networked] public NetworkBool FirstBloodTaken { get; set; }
        [Networked, Capacity(StructureCapacity)] public NetworkArray<NetworkId> Structures => default;
        [Networked, Capacity(StructureCapacity)] public NetworkArray<int> StructureRespawnTick => default;

        public IReadOnlyList<PlayerSlot> Slots => _slots;
        public MapLayout Map => _map;
        public GameRulesConfig Rules => _services.Rules;
        public MatchServices Services => _services;
        public WaveDirector Waves { get; private set; }
        public JungleDirector Jungle { get; private set; }

        public float ElapsedSeconds => Phase == MatchPhase.Lobby ? 0f : TickTime.TicksToSeconds((Phase == MatchPhase.Ended ? EndTick : Runner.Tick) - StartTick, Runner.TickRate);

        public override void Spawned()
        {
            _services = MatchServices.Of(Runner);
            _services.Attach(this, _map);
            _rewards = new MatchRewards(this);
            Waves = GetComponent<WaveDirector>();
            Jungle = GetComponent<JungleDirector>();
            if (HasStateAuthority)
            {
                Seed = (ulong)System.DateTime.UtcNow.Ticks;
                foreach (PlayerRef player in Runner.ActivePlayers) PlayerJoined(player);
            }
        }

        public override void Despawned(NetworkRunner runner, bool hasState) => MatchServices.Of(runner)?.Detach(this);

        public void Register(PlayerSlot slot)
        {
            if (!_slots.Contains(slot)) _slots.Add(slot);
            _slots.Sort((a, b) => a.Player.RawEncoded.CompareTo(b.Player.RawEncoded));
        }

        public void Unregister(PlayerSlot slot) => _slots.Remove(slot);

        public PlayerSlot SlotOf(PlayerRef player)
        {
            foreach (PlayerSlot slot in _slots)
                if (slot.Player == player) return slot;
            return null;
        }

        public bool HasRoom(Team team, PlayerSlot except)
        {
            int count = 0;
            foreach (PlayerSlot slot in _slots)
                if (slot != except && slot.Team == team) count++;
            return count < Rules.PlayersPerTeam;
        }

        public void PlayerJoined(PlayerRef player)
        {
            if (!HasStateAuthority || SlotOf(player) != null || _playerSlotPrefab == null) return;
            Team team = CountTeam(Team.Blue) <= CountTeam(Team.Red) ? Team.Blue : Team.Red;
            Runner.Spawn(_playerSlotPrefab, Vector3.zero, Quaternion.identity, player, (runner, obj) =>
            {
                PlayerSlot slot = obj.GetComponent<PlayerSlot>();
                slot.Player = player;
                slot.Team = team;
                slot.PlayerName = $"Joueur {player.PlayerId}";
                slot.Champion = 0;
                var spells = Rules.DefaultSummonerSpells;
                slot.Summoner1 = (short)(spells.Count > 0 ? _services.Content.SummonerSpells.IndexOf(spells[0].Id) : -1);
                slot.Summoner2 = (short)(spells.Count > 1 ? _services.Content.SummonerSpells.IndexOf(spells[1].Id) : -1);
                Register(slot);
            });
        }

        public void PlayerLeft(PlayerRef player)
        {
            if (!HasStateAuthority) return;
            PlayerSlot slot = SlotOf(player);
            if (slot == null) return;
            if (slot.ChampionUnit.IsValid && Runner.TryFindObject(slot.ChampionUnit, out NetworkObject champion)) Runner.Despawn(champion);
            Runner.Despawn(slot.Object);
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        public void RpcRequestStart(RpcInfo info = default)
        {
            bool fromHost = info.Source == Runner.LocalPlayer || info.Source == PlayerRef.None;
            if (fromHost) StartMatch();
        }

        public void StartMatch()
        {
            if (!HasStateAuthority || Phase != MatchPhase.Lobby) return;
            Phase = MatchPhase.Playing;
            StartTick = Runner.Tick;
            SpawnStructures();
            foreach (PlayerSlot slot in _slots) SpawnChampion(slot);
            Waves.Begin();
            Jungle.Begin();
        }

        public override void FixedUpdateNetwork()
        {
            if (!HasStateAuthority || Phase != MatchPhase.Playing) return;
            float elapsed = ElapsedSeconds;
            Waves.Simulate(elapsed);
            Jungle.Simulate(elapsed);
            SimulateChampions(elapsed);
            SimulateStructures(elapsed);
            CollectCorpses();
        }

        public void OnUnitKilled(Unit victim, Unit killer)
        {
            if (!HasStateAuthority) return;
            _rewards.Grant(victim, killer);
            if (victim.IsChampion)
            {
                ChampionProgress progress = victim.GetComponent<ChampionProgress>();
                progress.RespawnTick = Runner.Tick + TickTime.SecondsToTicks(Rules.RespawnSeconds(victim.Stats.Level), Runner.TickRate);
                return;
            }
            if (victim.Kind == UnitKind.Structure)
            {
                OnStructureDestroyed(victim);
                return;
            }
            if (victim.Kind == UnitKind.Minion || victim.Kind == UnitKind.Monster || victim.Kind == UnitKind.Summon)
                _corpses.Add((victim.Object, Runner.Tick + TickTime.SecondsToTicks(CorpseSeconds, Runner.TickRate)));
            if (victim.Kind == UnitKind.Monster) Jungle.OnMonsterKilled(victim, killer);
        }

        public bool FirstBlood()
        {
            if (FirstBloodTaken) return false;
            FirstBloodTaken = true;
            return true;
        }

        public void ReturnToBase(Unit unit)
        {
            if (!HasStateAuthority || _map == null) return;
            unit.Attack?.ClearTarget();
            unit.Motor?.Teleport(_map.Fountain(unit.Team));
        }

        public bool IsAtFountain(Unit unit) =>
            _map != null && UnitRegistry.FlatDistance(unit.Position, _map.Fountain(unit.Team)) <= Rules.FountainRadius;

        public bool IsInShopRange(Unit unit) =>
            _map != null && (!unit.IsAlive || UnitRegistry.FlatDistance(unit.Position, _map.Fountain(unit.Team)) <= Rules.ShopRadius);

        public bool IsInhibitorDown(Team team, int lane)
        {
            for (int i = 0; i < Structures.Length; i++)
            {
                if (!TryUnit(Structures[i], out Unit structure) || structure.Team != team || structure.Lane != lane) continue;
                if (IsInhibitor(structure) && !structure.IsAlive) return true;
            }
            return false;
        }

        public Unit SpawnUnit(UnitDefinition definition, Team team, Vector3 position, Quaternion rotation, PlayerRef owner, int lane = NoLane)
        {
            if (definition == null || definition.Prefab == null) return null;
            short index = (short)_services.Content.Units.IndexOf(definition.Id);
            if (index < 0) return null;
            NetworkObject obj = Runner.Spawn(definition.Prefab, position, rotation, owner, (runner, spawned) =>
            {
                Unit unit = spawned.GetComponent<Unit>();
                unit.Team = team;
                unit.DefinitionIndex = index;
                unit.Lane = (byte)lane;
            });
            return obj != null ? obj.GetComponent<Unit>() : null;
        }

        public void SpawnSummon(Unit owner, UnitDefinition definition, int count, float durationSeconds, Vector3 point)
        {
            for (int i = 0; i < count; i++)
            {
                Unit summon = SpawnUnit(definition, owner.Team, point + Quaternion.AngleAxis(360f * i / Mathf.Max(1, count), Vector3.up) * Vector3.forward, Quaternion.identity, PlayerRef.None);
                if (summon != null && summon.TryGetComponent(out AI.MinionBrain brain)) brain.MakeSummon(owner, durationSeconds);
            }
        }

        private void SpawnStructures()
        {
            if (_map == null) return;
            IReadOnlyList<StructurePlacement> placements = _map.Structures;
            for (int i = 0; i < placements.Count && i < StructureCapacity; i++)
            {
                StructurePlacement placement = placements[i];
                Unit unit = SpawnUnit(placement.Definition, placement.Team, placement.Point.position, placement.Point.rotation, PlayerRef.None, placement.Lane < 0 ? NoLane : placement.Lane);
                Structures.Set(i, unit != null ? unit.Object.Id : default);
                if (unit != null && unit.Kind == UnitKind.Tower) unit.Fortified = Rules.EarlyTowerDefenseSeconds > 0f;
            }
            for (int i = 0; i < placements.Count && i < StructureCapacity; i++)
            {
                if (!TryUnit(Structures[i], out Unit unit)) continue;
                StructurePlacement placement = placements[i];
                unit.Protection = placement.Protection;
                for (int p = 0; p < placement.Protectors.Count && p < unit.Protectors.Length; p++)
                {
                    int protector = placement.Protectors[p];
                    if (protector >= 0 && protector < StructureCapacity) unit.Protectors.Set(p, Structures[protector]);
                }
            }
        }

        private void SpawnChampion(PlayerSlot slot)
        {
            var champions = _services.Content.Champions;
            if (slot.Champion < 0 || slot.Champion >= champions.Count || _map == null) return;
            ChampionDefinition definition = champions[slot.Champion];
            Vector3 fountain = _map.Fountain(slot.Team);
            Unit unit = SpawnUnit(definition, slot.Team, fountain, Quaternion.identity, slot.Player);
            if (unit == null) return;
            slot.ChampionUnit = unit.Object.Id;
            ChampionProgress progress = unit.GetComponent<ChampionProgress>();
            progress.Owner = slot.Player;
            progress.PlayerName = slot.PlayerName;
            progress.Initialize(unit, Rules.StartingGold);
            var spells = new List<SummonerSpellDefinition>();
            var allSpells = _services.Content.SummonerSpells;
            if (slot.Summoner1 >= 0 && slot.Summoner1 < allSpells.Count) spells.Add(allSpells[slot.Summoner1]);
            if (slot.Summoner2 >= 0 && slot.Summoner2 < allSpells.Count) spells.Add(allSpells[slot.Summoner2]);
            unit.Caster?.Setup(definition, spells, Rules.RecallAbility);
        }

        private void SimulateChampions(float elapsed)
        {
            float dt = Runner.DeltaTime;
            bool passiveGold = elapsed >= Rules.PassiveGoldStartSeconds;
            foreach (PlayerSlot slot in _slots)
            {
                if (!TryUnit(slot.ChampionUnit, out Unit champion)) continue;
                ChampionProgress progress = champion.GetComponent<ChampionProgress>();
                if (passiveGold) progress.AddGold(Rules.PassiveGoldPerSecond * dt);
                if (!champion.IsAlive)
                {
                    if (progress.RespawnTick > 0 && Runner.Tick >= progress.RespawnTick) Respawn(champion, progress);
                    continue;
                }
                if (IsAtFountain(champion))
                {
                    champion.Health.Heal(champion.Health.Max * Rules.FountainRegenPerSecond * dt, null);
                    champion.Resource?.Restore(champion.Resource.Max * Rules.FountainRegenPerSecond * dt);
                }
            }
        }

        private void Respawn(Unit champion, ChampionProgress progress)
        {
            progress.RespawnTick = 0;
            champion.Motor?.Teleport(_map.Fountain(champion.Team));
            champion.Health.Revive();
            champion.Resource?.Fill();
        }

        private void SimulateStructures(float elapsed)
        {
            bool fortified = elapsed < Rules.EarlyTowerDefenseSeconds;
            for (int i = 0; i < Structures.Length; i++)
            {
                if (!TryUnit(Structures[i], out Unit structure)) continue;
                if (structure.Kind == UnitKind.Tower && structure.Fortified && !fortified) structure.Fortified = false;
                int respawn = StructureRespawnTick[i];
                if (respawn > 0 && Runner.Tick >= respawn)
                {
                    StructureRespawnTick.Set(i, 0);
                    structure.Health.Revive();
                }
            }
        }

        private void OnStructureDestroyed(Unit structure)
        {
            if (IsCore(structure))
            {
                Phase = MatchPhase.Ended;
                EndTick = Runner.Tick;
                Winner = TeamRules.Opponent(structure.Team);
                return;
            }
            for (int i = 0; i < Structures.Length; i++)
                if (Structures[i] == structure.Object.Id)
                    StructureRespawnTick.Set(i, Runner.Tick + TickTime.SecondsToTicks(Rules.InhibitorRespawnSeconds, Runner.TickRate));
        }

        public static bool IsCore(Unit unit) => unit.Kind == UnitKind.Structure && unit.Lane == NoLane;

        public static bool IsInhibitor(Unit unit) => unit.Kind == UnitKind.Structure && unit.Lane != NoLane;

        private void CollectCorpses()
        {
            for (int i = _corpses.Count - 1; i >= 0; i--)
            {
                if (Runner.Tick < _corpses[i].tick) continue;
                if (_corpses[i].obj != null) Runner.Despawn(_corpses[i].obj);
                _corpses.RemoveAt(i);
            }
        }

        private int CountTeam(Team team)
        {
            int count = 0;
            foreach (PlayerSlot slot in _slots)
                if (slot.Team == team) count++;
            return count;
        }

        private bool TryUnit(NetworkId id, out Unit unit)
        {
            unit = null;
            if (!id.IsValid || !Runner.TryFindObject(id, out NetworkObject obj)) return false;
            unit = obj.GetComponent<Unit>();
            return unit != null;
        }
    }
}

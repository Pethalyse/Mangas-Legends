using System.Collections.Generic;
using Fusion;
using MangaLegends.Core.Simulation;
using MangaLegends.Data.Definitions;
using MangaLegends.Simulation.AI;
using MangaLegends.Simulation.Units;
using UnityEngine;

namespace MangaLegends.Simulation.Match
{
    public sealed class JungleDirector : NetworkBehaviour
    {
        private const int CampCapacity = 20;
        private const int MonstersPerCamp = 4;

        private MatchController _match;
        private readonly List<UnitDefinition> _single = new List<UnitDefinition>();

        [Networked, Capacity(CampCapacity)] public NetworkArray<int> NextSpawnTick => default;
        [Networked, Capacity(CampCapacity * MonstersPerCamp)] public NetworkArray<NetworkId> Monsters => default;
        [Networked, Capacity(CampCapacity)] public NetworkArray<short> SpawnCount => default;

        public override void Spawned() => _match = GetComponent<MatchController>();

        public void Begin()
        {
            if (_match.Map == null) return;
            for (int camp = 0; camp < _match.Map.Camps.Count && camp < CampCapacity; camp++)
                NextSpawnTick.Set(camp, Runner.Tick + TickTime.SecondsToTicks(_match.Map.Camps[camp].FirstSpawnSeconds, Runner.TickRate));
        }

        public void Simulate(float elapsed)
        {
            if (_match.Map == null) return;
            for (int camp = 0; camp < _match.Map.Camps.Count && camp < CampCapacity; camp++)
            {
                CampPlacement placement = _match.Map.Camps[camp];
                if (placement.DespawnSeconds > 0f && elapsed >= placement.DespawnSeconds)
                {
                    NextSpawnTick.Set(camp, 0);
                    DespawnCamp(camp);
                    continue;
                }
                int spawnTick = NextSpawnTick[camp];
                if (spawnTick <= 0 || Runner.Tick < spawnTick) continue;
                NextSpawnTick.Set(camp, 0);
                SpawnCamp(camp);
            }
        }

        public void OnMonsterKilled(Unit monster, Unit killer)
        {
            for (int camp = 0; camp < _match.Map.Camps.Count && camp < CampCapacity; camp++)
            {
                if (!Contains(camp, monster.Object.Id)) continue;
                if (!AllDead(camp)) return;
                float respawn = _match.Map.Camps[camp].RespawnSeconds;
                if (respawn > 0f) NextSpawnTick.Set(camp, Runner.Tick + TickTime.SecondsToTicks(respawn, Runner.TickRate));
                return;
            }
        }

        private void SpawnCamp(int camp)
        {
            CampPlacement placement = _match.Map.Camps[camp];
            IReadOnlyList<UnitDefinition> monsters = MonstersFor(camp, placement);
            SpawnCount.Set(camp, (short)(SpawnCount[camp] + 1));
            for (int i = 0; i < MonstersPerCamp; i++)
            {
                Monsters.Set(camp * MonstersPerCamp + i, default);
                if (i >= monsters.Count) continue;
                Vector3 offset = monsters.Count > 1 ? Quaternion.AngleAxis(360f * i / monsters.Count, Vector3.up) * Vector3.forward * 1.5f : Vector3.zero;
                Vector3 position = placement.Point.position + offset;
                Unit unit = _match.SpawnUnit(monsters[i], Team.Neutral, position, placement.Point.rotation, PlayerRef.None);
                if (unit == null) continue;
                if (monsters[i] is MonsterDefinition monster && monster.ScalesWithAverageLevel)
                {
                    unit.Stats.Level = (byte)_match.AverageChampionLevel();
                    unit.Stats.Refresh();
                    unit.Health.SetCurrent(unit.Health.Max);
                }
                Monsters.Set(camp * MonstersPerCamp + i, unit.Object.Id);
                if (unit.TryGetComponent(out MonsterBrain brain)) brain.SetHome(position, placement.LeashRadius);
            }
        }

        // Drake pits draw their element (no repeat, then the map element), then hold the elder once a team owns the soul.
        private IReadOnlyList<UnitDefinition> MonstersFor(int camp, CampPlacement placement)
        {
            _single.Clear();
            if (placement.AfterSoul != null && _match.AnySoul) _single.Add(placement.AfterSoul);
            else if (placement.Rotation.Count > 0) _single.Add(placement.Rotation[ElementRotation.Pick(_match.Seed ^ (ulong)camp, SpawnCount[camp], placement.Rotation.Count, _match.Rules.DrakeElementLocksAfter)]);
            else return placement.Monsters;
            return _single;
        }

        private void DespawnCamp(int camp)
        {
            for (int i = 0; i < MonstersPerCamp; i++)
            {
                NetworkId id = Monsters[camp * MonstersPerCamp + i];
                if (!id.IsValid) continue;
                Monsters.Set(camp * MonstersPerCamp + i, default);
                if (Runner.TryFindObject(id, out NetworkObject obj)) Runner.Despawn(obj);
            }
        }

        private bool Contains(int camp, NetworkId id)
        {
            for (int i = 0; i < MonstersPerCamp; i++)
                if (Monsters[camp * MonstersPerCamp + i] == id) return true;
            return false;
        }

        private bool AllDead(int camp)
        {
            for (int i = 0; i < MonstersPerCamp; i++)
            {
                NetworkId id = Monsters[camp * MonstersPerCamp + i];
                if (!id.IsValid || !Runner.TryFindObject(id, out NetworkObject obj)) continue;
                Unit unit = obj.GetComponent<Unit>();
                if (unit != null && unit.IsAlive) return false;
            }
            return true;
        }
    }
}

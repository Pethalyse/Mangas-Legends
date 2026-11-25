using Fusion;
using MangaLegends.Data.Definitions;
using MangaLegends.Simulation.Abilities;
using MangaLegends.Simulation.Champions;
using MangaLegends.Simulation.Units;
using UnityEngine;

namespace MangaLegends.Simulation.Match
{
    // Training game tools (solo only): levels, unlimited gold, no cooldowns, dummies to cast on.
    // The UI calls them directly because in solo the local peer is the state authority.
    [RequireComponent(typeof(MatchController))]
    public sealed class TrainingTools : NetworkBehaviour
    {
        private const int DummyCapacity = 16;
        private const float UnlimitedGold = 99999f;
        private const float DummyReviveSeconds = 3f;

        private MatchController _match;

        [Networked] public NetworkBool InfiniteGold { get; set; }
        [Networked] public NetworkBool NoCooldowns { get; set; }
        [Networked, Capacity(DummyCapacity)] public NetworkArray<NetworkId> Dummies => default;

        public bool Available => Runner != null && Runner.GameMode == GameMode.Single && HasStateAuthority;

        private void Awake() => _match = GetComponent<MatchController>();

        public override void FixedUpdateNetwork()
        {
            if (!Available || _match.Phase != MatchPhase.Playing) return;
            ReviveDummies();
            foreach (Unit unit in _match.Services.Units.All)
            {
                if (unit == null || !unit.IsChampion || unit.Object.InputAuthority == PlayerRef.None) continue;
                if (InfiniteGold && unit.TryGetComponent(out ChampionProgress progress) && progress.Gold < UnlimitedGold) progress.AddGold(UnlimitedGold - progress.Gold);
                if (!NoCooldowns || unit.Caster == null) continue;
                for (int slot = (int)CasterSlot.A; slot <= (int)CasterSlot.Trinket; slot++) unit.Caster.ReduceCooldown((CasterSlot)slot, 0f);
                unit.Resource?.Fill();
            }
        }

        // Like the LoL practice dummy, a killed dummy stands up again after a moment.
        private void ReviveDummies()
        {
            int delay = Core.Simulation.TickTime.SecondsToTicks(DummyReviveSeconds, Runner.TickRate);
            for (int i = 0; i < Dummies.Length; i++)
            {
                if (!Dummies[i].IsValid || !Runner.TryFindObject(Dummies[i], out NetworkObject obj)) continue;
                Unit dummy = obj.GetComponent<Unit>();
                if (dummy.IsAlive || Runner.Tick < dummy.Health.DeathTick + delay) continue;
                dummy.Health.Revive();
            }
        }

        public void AddLevels(Unit champion, int levels)
        {
            if (!Available || champion == null || !champion.TryGetComponent(out ChampionProgress progress)) return;
            int target = Mathf.Min(_match.Rules.MaxLevel, champion.Stats.Level + levels);
            if (target > champion.Stats.Level) progress.AddExperience(Mathf.Max(1, _match.Rules.ExperienceToReach(target) - progress.Experience));
        }

        public void SpawnDummy(Team team, Vector3 point)
        {
            ChampionDefinition dummy = _match.Rules.TrainingDummy;
            if (!Available || dummy == null) return;
            int free = -1;
            for (int i = 0; i < Dummies.Length; i++)
                if (!Dummies[i].IsValid || !Runner.TryFindObject(Dummies[i], out _)) { free = i; break; }
            if (free < 0) return;
            Unit unit = _match.SpawnUnit(dummy, team, point, Quaternion.identity, PlayerRef.None);
            if (unit != null) Dummies.Set(free, unit.Object.Id);
        }

        public void ClearDummies()
        {
            if (!Available) return;
            for (int i = 0; i < Dummies.Length; i++)
            {
                if (Dummies[i].IsValid && Runner.TryFindObject(Dummies[i], out NetworkObject obj)) Runner.Despawn(obj);
                Dummies.Set(i, default);
            }
        }
    }
}

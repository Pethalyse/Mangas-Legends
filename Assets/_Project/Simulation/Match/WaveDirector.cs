using System.Collections.Generic;
using Fusion;
using MangaLegends.Core.Simulation;
using MangaLegends.Data.Definitions;
using MangaLegends.Simulation.AI;
using MangaLegends.Simulation.Units;
using UnityEngine;

namespace MangaLegends.Simulation.Match
{
    public sealed class WaveDirector : NetworkBehaviour
    {
        private const float LateSiegeSeconds = 840f;
        private const float LateCasterSeconds = 1800f;

        private readonly List<UnitDefinition> _wave = new List<UnitDefinition>();
        private MatchController _match;

        [Networked] public int WaveIndex { get; set; }
        [Networked] public int NextWaveTick { get; set; }
        [Networked] public int SpawnStep { get; set; }
        [Networked] public int NextSpawnTick { get; set; }
        [Networked] public float WaveElapsed { get; set; }

        public override void Spawned() => _match = GetComponent<MatchController>();

        public void Begin()
        {
            WaveIndex = 0;
            NextWaveTick = Runner.Tick + TickTime.SecondsToTicks(_match.Rules.FirstWaveSeconds, Runner.TickRate);
        }

        // Minions leave the base one at a time, in single file, as in LoL.
        public void Simulate(float elapsed)
        {
            if (_match.Map == null) return;
            if (NextSpawnTick > 0 && Runner.Tick >= NextSpawnTick) SpawnNextInLine();
            if (NextWaveTick <= 0 || Runner.Tick < NextWaveTick) return;
            WaveIndex++;
            WaveElapsed = elapsed;
            SpawnStep = 0;
            SpawnNextInLine();
            NextWaveTick = Runner.Tick + TickTime.SecondsToTicks(_match.Rules.WaveIntervalAt(elapsed), Runner.TickRate);
        }

        private void SpawnNextInLine()
        {
            bool any = false;
            for (int lane = 0; lane < _match.Map.Lanes.Count; lane++)
                foreach (Team team in new[] { Team.Blue, Team.Red })
                {
                    BuildWave(lane, team, WaveElapsed);
                    if (SpawnStep >= _wave.Count) continue;
                    Spawn(_wave[SpawnStep], lane, team);
                    any = true;
                }
            SpawnStep++;
            NextSpawnTick = any ? Runner.Tick + Mathf.Max(1, TickTime.SecondsToTicks(_match.Rules.MinionSpawnIntervalSeconds, Runner.TickRate)) : 0;
        }

        private void BuildWave(int lane, Team team, float elapsed)
        {
            GameRulesConfig rules = _match.Rules;
            Team opponent = TeamRules.Opponent(team);
            bool siegeWave = WaveIndex >= rules.FirstSiegeWave && (WaveIndex - rules.FirstSiegeWave) % rules.SiegeEveryNWavesAt(elapsed) == 0;
            bool laneOpen = _match.IsInhibitorDown(opponent, lane);
            bool allOpen = AllInhibitorsDown(opponent);
            int megas = allOpen ? 2 : laneOpen ? 1 : 0;
            bool siege = siegeWave && megas == 0;
            int melee = rules.MeleePerWave - (siege && elapsed >= LateSiegeSeconds ? 1 : 0);
            int casters = rules.CasterPerWave - (elapsed >= LateCasterSeconds ? 1 : 0);

            _wave.Clear();
            for (int i = 0; i < melee; i++) _wave.Add(rules.MeleeMinion);
            for (int i = 0; i < megas; i++) _wave.Add(rules.MegaMinion);
            if (siege) _wave.Add(rules.SiegeMinion);
            for (int i = 0; i < casters; i++) _wave.Add(rules.CasterMinion);
        }

        private void Spawn(UnitDefinition definition, int lane, Team team)
        {
            LanePath path = _match.Map.Lanes[lane];
            Vector3 start = path.WaypointFor(team, 0);
            Vector3 next = path.WaypointFor(team, 1);
            Vector3 direction = next - start;
            direction.y = 0f;
            direction = direction.sqrMagnitude > 0.0001f ? direction.normalized : Vector3.forward;
            Unit unit = _match.SpawnUnit(definition, team, start, Quaternion.LookRotation(direction), PlayerRef.None, lane);
            if (unit != null && unit.TryGetComponent(out MinionBrain brain)) brain.AssignLane(lane);
        }

        private bool AllInhibitorsDown(Team team)
        {
            for (int lane = 0; lane < _match.Map.Lanes.Count; lane++)
                if (!_match.IsInhibitorDown(team, lane)) return false;
            return _match.Map.Lanes.Count > 0;
        }
    }
}

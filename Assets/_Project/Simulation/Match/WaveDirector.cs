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

        private MatchController _match;

        [Networked] public int WaveIndex { get; set; }
        [Networked] public int NextWaveTick { get; set; }

        public override void Spawned() => _match = GetComponent<MatchController>();

        public void Begin()
        {
            WaveIndex = 0;
            NextWaveTick = Runner.Tick + TickTime.SecondsToTicks(_match.Rules.FirstWaveSeconds, Runner.TickRate);
        }

        public void Simulate(float elapsed)
        {
            if (NextWaveTick <= 0 || Runner.Tick < NextWaveTick || _match.Map == null) return;
            WaveIndex++;
            for (int lane = 0; lane < _match.Map.Lanes.Count; lane++)
            {
                SpawnWave(lane, Team.Blue, elapsed);
                SpawnWave(lane, Team.Red, elapsed);
            }
            NextWaveTick = Runner.Tick + TickTime.SecondsToTicks(_match.Rules.WaveIntervalAt(elapsed), Runner.TickRate);
        }

        private void SpawnWave(int lane, Team team, float elapsed)
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

            int order = 0;
            for (int i = 0; i < melee; i++) Spawn(rules.MeleeMinion, lane, team, order++);
            for (int i = 0; i < megas; i++) Spawn(rules.MegaMinion, lane, team, order++);
            if (siege) Spawn(rules.SiegeMinion, lane, team, order++);
            for (int i = 0; i < casters; i++) Spawn(rules.CasterMinion, lane, team, order++);
        }

        private void Spawn(UnitDefinition definition, int lane, Team team, int order)
        {
            LanePath path = _match.Map.Lanes[lane];
            Vector3 start = path.WaypointFor(team, 0);
            Vector3 next = path.WaypointFor(team, 1);
            Vector3 direction = next - start;
            direction.y = 0f;
            direction = direction.sqrMagnitude > 0.0001f ? direction.normalized : Vector3.forward;
            Vector3 side = Vector3.Cross(Vector3.up, direction) * ((order % 2 == 0 ? 1f : -1f) * 0.6f);
            Vector3 position = start - direction * (_match.Rules.MinionSpawnSpacing * (order / 2)) + side;
            Unit unit = _match.SpawnUnit(definition, team, position, Quaternion.LookRotation(direction), PlayerRef.None, lane);
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

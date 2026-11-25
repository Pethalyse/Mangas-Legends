using Fusion;
using MangaLegends.Core.Simulation;
using MangaLegends.Simulation.AI;
using MangaLegends.Simulation.Units;
using UnityEngine;

namespace MangaLegends.Simulation.Match
{
    public sealed class JungleDirector : NetworkBehaviour
    {
        private const int CampCapacity = 16;
        private const int MonstersPerCamp = 4;

        private MatchController _match;

        [Networked, Capacity(CampCapacity)] public NetworkArray<int> NextSpawnTick => default;
        [Networked, Capacity(CampCapacity * MonstersPerCamp)] public NetworkArray<NetworkId> Monsters => default;

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
            for (int i = 0; i < MonstersPerCamp; i++)
            {
                Monsters.Set(camp * MonstersPerCamp + i, default);
                if (i >= placement.Monsters.Count) continue;
                Vector3 offset = placement.Monsters.Count > 1 ? Quaternion.AngleAxis(360f * i / placement.Monsters.Count, Vector3.up) * Vector3.forward * 1.5f : Vector3.zero;
                Vector3 position = placement.Point.position + offset;
                Unit unit = _match.SpawnUnit(placement.Monsters[i], Team.Neutral, position, placement.Point.rotation, PlayerRef.None);
                if (unit == null) continue;
                Monsters.Set(camp * MonstersPerCamp + i, unit.Object.Id);
                if (unit.TryGetComponent(out MonsterBrain brain)) brain.SetHome(position, placement.LeashRadius);
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

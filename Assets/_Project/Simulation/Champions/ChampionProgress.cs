using Fusion;
using MangaLegends.Data.Definitions;
using MangaLegends.Simulation.Units;
using UnityEngine;

namespace MangaLegends.Simulation.Champions
{
    public sealed class ChampionProgress : NetworkBehaviour
    {
        private Unit _unit;

        [Networked] public PlayerRef Owner { get; set; }
        [Networked] public NetworkString<_32> PlayerName { get; set; }
        [Networked] public int Experience { get; set; }
        [Networked] public float Gold { get; set; }
        [Networked] public int TotalGold { get; set; }
        [Networked] public short Kills { get; set; }
        [Networked] public short Deaths { get; set; }
        [Networked] public short Assists { get; set; }
        [Networked] public short MinionKills { get; set; }
        [Networked] public short KillStreak { get; set; }
        [Networked] public int RespawnTick { get; set; }

        public int GoldAmount => Mathf.FloorToInt(Gold);
        public int Level => _unit != null ? _unit.Stats.Level : 1;
        public float RespawnRemaining => RespawnTick > 0 ? Mathf.Max(0f, (RespawnTick - Runner.Tick) / (float)Runner.TickRate) : 0f;

        public float LevelProgress
        {
            get
            {
                GameRulesConfig rules = _unit.Services.Rules;
                if (Level >= rules.MaxLevel) return 1f;
                int floor = rules.ExperienceToReach(Level);
                int next = rules.ExperienceToReach(Level + 1);
                return next > floor ? Mathf.Clamp01((Experience - floor) / (float)(next - floor)) : 1f;
            }
        }

        public void Initialize(Unit unit, int startingGold)
        {
            _unit = unit;
            if (!HasStateAuthority) return;
            Gold = startingGold;
            TotalGold = startingGold;
        }

        public override void Spawned() => _unit = GetComponent<Unit>();

        public void AddGold(float amount)
        {
            if (!HasStateAuthority || amount <= 0f) return;
            int before = GoldAmount;
            Gold += amount;
            TotalGold += GoldAmount - before;
        }

        public bool SpendGold(int amount)
        {
            if (!HasStateAuthority || amount > GoldAmount) return false;
            Gold -= amount;
            return true;
        }

        public void AddExperience(int amount)
        {
            if (!HasStateAuthority || amount <= 0) return;
            GameRulesConfig rules = _unit.Services.Rules;
            Experience += amount;
            while (_unit.Stats.Level < rules.MaxLevel && Experience >= rules.ExperienceToReach(_unit.Stats.Level + 1))
                LevelUp();
        }

        private void LevelUp()
        {
            float healthRatio = _unit.Health.Ratio;
            float resourceRatio = _unit.Resource != null ? _unit.Resource.Ratio : 0f;
            _unit.Stats.Level++;
            _unit.Stats.Refresh();
            _unit.Health.SetCurrent(_unit.Health.Max * healthRatio);
            if (_unit.Resource != null) _unit.Resource.Current = _unit.Resource.Max * resourceRatio;
        }
    }
}

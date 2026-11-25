using System.Collections.Generic;
using Fusion;
using MangaLegends.Core.Stats;
using MangaLegends.Data.Serialization;
using UnityEngine;

namespace MangaLegends.Simulation.Units
{
    public interface IStatModifierSource
    {
        int ModifierVersion { get; }
        void CollectModifiers(List<StatModifier> into);
    }

    public sealed class UnitStats : NetworkBehaviour, IStatReader
    {
        private const int FortificationSource = 9000;

        private readonly List<StatModifier> _buffer = new List<StatModifier>();
        private Unit _unit;
        private IStatModifierSource[] _sources;
        private StatBlock _block;
        private int _builtVersion = int.MinValue;

        [Networked] public byte Level { get; set; }

        public StatBlock Block
        {
            get
            {
                Refresh();
                return _block;
            }
        }

        public void Initialize(Unit unit)
        {
            _unit = unit;
            _sources = GetComponents<IStatModifierSource>();
            if (HasStateAuthority && Level == 0) Level = 1;
            _block = null;
            _builtVersion = int.MinValue;
        }

        public float Get(StatType stat)
        {
            float value = Block?.Get(stat) ?? 0f;
            if (stat == StatType.AttackSpeed && _unit != null && _unit.Services?.Rules != null) value = Mathf.Min(value, _unit.Services.Rules.MaxAttackSpeed);
            if (stat == StatType.MoveSpeed) value = Mathf.Max(0f, value);
            return value;
        }

        public float GetBase(StatType stat) => Block?.GetBase(stat) ?? 0f;
        public float GetBonus(StatType stat) => Get(stat) - GetBase(stat);

        public void Refresh()
        {
            if (_unit == null || _unit.Definition == null) return;
            if (_block == null) _block = new StatBlock(_unit.Definition.Stats.ToBaseline());

            int version = Mathf.Max(1, (int)Level) * 7919 + (_unit.Fortified ? 1 : 0);
            if (_sources != null)
                foreach (IStatModifierSource source in _sources) version = unchecked(version * 31 + source.ModifierVersion);
            if (version == _builtVersion) return;

            _builtVersion = version;
            _block.SetLevel(Mathf.Max(1, (int)Level));
            _block.ClearModifiers();
            _buffer.Clear();
            if (_sources != null)
                foreach (IStatModifierSource source in _sources) source.CollectModifiers(_buffer);
            if (_unit.Fortified && _unit.Services?.Rules != null)
                foreach (StatModifierData modifier in _unit.Services.Rules.EarlyTowerModifiers) _buffer.Add(modifier.ToModifier(FortificationSource));
            foreach (StatModifier modifier in _buffer) _block.AddModifier(modifier);
        }
    }
}

using System.Collections.Generic;

namespace MangaLegends.Core.Combat
{
    public sealed class ShieldPool
    {
        private readonly List<Shield> _shields = new List<Shield>();

        public IReadOnlyList<Shield> Shields => _shields;

        public float Total
        {
            get
            {
                float total = 0f;
                foreach (Shield shield in _shields) total += shield.Remaining;
                return total;
            }
        }

        public bool BlocksCrowdControl
        {
            get
            {
                foreach (Shield shield in _shields)
                    if (shield.BlocksCrowdControl) return true;
                return false;
            }
        }

        public void Add(Shield shield)
        {
            if (shield.Remaining <= 0f) return;
            _shields.Add(shield);
        }

        public bool Remove(int id)
        {
            int index = _shields.FindIndex(s => s.Id == id);
            if (index < 0) return false;
            _shields.RemoveAt(index);
            return true;
        }

        public void RemoveExpired(int currentTick)
        {
            _shields.RemoveAll(s => s.ExpiresAtTick > 0 && s.ExpiresAtTick <= currentTick);
        }

        public float Absorb(float amount, DamageType type)
        {
            float remaining = amount;
            remaining = AbsorbWith(remaining, type, typedOnly: true);
            remaining = AbsorbWith(remaining, type, typedOnly: false);
            _shields.RemoveAll(s => s.Remaining <= 0f);
            return amount - remaining;
        }

        // Typed shields are consumed before generic ones so that a generic shield keeps protecting against every damage type.
        private float AbsorbWith(float amount, DamageType type, bool typedOnly)
        {
            for (int i = 0; i < _shields.Count && amount > 0f; i++)
            {
                Shield shield = _shields[i];
                bool isTyped = shield.Filter != ShieldFilter.Any;
                if (isTyped != typedOnly || !shield.Accepts(type)) continue;

                float absorbed = shield.Remaining < amount ? shield.Remaining : amount;
                shield.Remaining -= absorbed;
                amount -= absorbed;
                _shields[i] = shield;
            }
            return amount;
        }
    }
}

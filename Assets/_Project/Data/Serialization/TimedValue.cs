using System;
using UnityEngine;

namespace MangaLegends.Data.Serialization
{
    [Serializable]
    public struct TimedValue
    {
        [SerializeField, Min(0f)] private float _fromSeconds;
        [SerializeField] private float _value;

        public TimedValue(float fromSeconds, float value)
        {
            _fromSeconds = fromSeconds;
            _value = value;
        }

        public float FromSeconds => _fromSeconds;
        public float Value => _value;

        public static float At(TimedValue[] steps, float elapsedSeconds)
        {
            if (steps == null || steps.Length == 0) return 0f;
            float result = steps[0]._value;
            foreach (TimedValue step in steps)
                if (elapsedSeconds >= step._fromSeconds) result = step._value;
            return result;
        }
    }
}

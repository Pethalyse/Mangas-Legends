namespace MangaLegends.Core.Simulation
{
    public struct DeterministicRandom
    {
        private ulong _state;

        public DeterministicRandom(ulong seed)
        {
            _state = seed;
        }

        public static DeterministicRandom ForTick(ulong matchSeed, int tick, int salt) =>
            new DeterministicRandom(matchSeed ^ ((ulong)(uint)tick << 32) ^ (uint)salt);

        public ulong NextULong()
        {
            unchecked
            {
                _state += 0x9E3779B97F4A7C15UL;
                ulong z = _state;
                z = (z ^ (z >> 30)) * 0xBF58476D1CE4E5B9UL;
                z = (z ^ (z >> 27)) * 0x94D049BB133111EBUL;
                return z ^ (z >> 31);
            }
        }

        public float NextFloat() => (NextULong() >> 40) * (1f / (1UL << 24));

        public bool Roll(float probability) => probability > 0f && (probability >= 1f || NextFloat() < probability);
    }
}

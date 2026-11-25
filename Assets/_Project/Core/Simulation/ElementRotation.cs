namespace MangaLegends.Core.Simulation
{
    // Genre rule for elemental drakes: the first spawns are drawn without repeats, then the element drawn at
    // spawn index `lockedFrom` (the map's element) repeats for every later spawn.
    public static class ElementRotation
    {
        public static int Pick(ulong seed, int spawnIndex, int elementCount, int lockedFrom)
        {
            if (elementCount <= 1) return 0;
            int[] order = new int[elementCount];
            for (int i = 0; i < elementCount; i++) order[i] = i;
            DeterministicRandom random = new DeterministicRandom(seed ^ 0xD8A6E5UL);
            for (int i = elementCount - 1; i > 0; i--)
            {
                int j = (int)(random.NextULong() % (ulong)(i + 1));
                int swap = order[i];
                order[i] = order[j];
                order[j] = swap;
            }
            int clampedLock = lockedFrom < 0 ? 0 : lockedFrom >= elementCount ? elementCount - 1 : lockedFrom;
            int index = spawnIndex < 0 ? 0 : spawnIndex > clampedLock ? clampedLock : spawnIndex;
            return order[index];
        }
    }
}

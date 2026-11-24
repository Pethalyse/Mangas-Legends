using System;
using System.Collections.Generic;

namespace MangaLegends.Core.Content
{
    public sealed class ContentIndex<T> where T : class, IContentEntry
    {
        private readonly List<T> _byIndex;
        private readonly Dictionary<string, int> _indexById;

        // Entries are sorted by id so that every peer assigns the same network index regardless of asset load order.
        public ContentIndex(IEnumerable<T> entries)
        {
            if (entries == null) throw new ArgumentNullException(nameof(entries));
            _byIndex = new List<T>();
            foreach (T entry in entries)
                if (entry != null) _byIndex.Add(entry);
            _byIndex.Sort((a, b) => string.CompareOrdinal(a.Id, b.Id));

            _indexById = new Dictionary<string, int>(_byIndex.Count, StringComparer.Ordinal);
            for (int i = 0; i < _byIndex.Count; i++)
            {
                string id = _byIndex[i].Id;
                if (!ContentId.IsValid(id)) throw new ArgumentException($"Invalid content id '{id}'.");
                if (_indexById.ContainsKey(id)) throw new ArgumentException($"Duplicate content id '{id}'.");
                _indexById.Add(id, i);
            }
        }

        public int Count => _byIndex.Count;
        public IReadOnlyList<T> All => _byIndex;

        public T this[int index] => _byIndex[index];

        public bool TryGet(string id, out T entry)
        {
            if (id != null && _indexById.TryGetValue(id, out int index))
            {
                entry = _byIndex[index];
                return true;
            }
            entry = null;
            return false;
        }

        public int IndexOf(string id) => id != null && _indexById.TryGetValue(id, out int index) ? index : -1;
    }
}

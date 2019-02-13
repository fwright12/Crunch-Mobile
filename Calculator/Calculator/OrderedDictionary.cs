using System;
using System.Collections.Generic;
using System.Text;

namespace Crunch
{
    /// <summary>
    /// A generic OrderedDictionary implemented as a list of KeyValuePairs. Has O(1) lookup of keys
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    public class OrderedDictionary<TKey, TValue>
    {
        private List<KeyValuePair<TKey, TValue>> list = new List<KeyValuePair<TKey, TValue>>();
        private HashSet<TKey> hash = new HashSet<TKey>();

        public int Count => list.Count;

        public KeyValuePair<TKey, TValue> this[int index] => list[index];

        public bool ContainsKey(TKey key) => hash.Contains(key);
        public void Add(TKey key, TValue value) => Insert(list.Count, key, value);
        public void Insert(int index, TKey key, TValue value)
        {
            list.Insert(index, new KeyValuePair<TKey, TValue>(key, value));
            hash.Add(key);
        }
    }
}

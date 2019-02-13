using System;
using System.Collections.Generic;
using System.Text;

namespace Crunch
{
    public enum TrieContains { Full, Partial, No };

    public class TrieNode<TValue>
    {
        public TValue Value => value;

        private TValue value = default(TValue);
        private Dictionary<char, TrieNode<TValue>> dict = new Dictionary<char, TrieNode<TValue>>();

        public TrieContains Contains(string key)
        {
            TrieNode<TValue> node = this;

            foreach (char c in key)
            {
                if (!node.Contains(c))
                {
                    return TrieContains.No;
                }
                node = node.dict[c];
            }

            return !EqualityComparer<TValue>.Default.Equals(node.Value, default(TValue)) ? TrieContains.Full : TrieContains.Partial;
        }

        public void Insert(string key, TValue value)
        {
            TrieNode<TValue> node = this;

            foreach(char c in key)
            {
                if (!node.Contains(c))
                {
                    node.dict.Add(c, new TrieNode<TValue>());
                }
                node = node.dict[c];
            }

            node.value = value;
        }

        private bool Contains(char c) => dict.ContainsKey(c);
    }

    public class OrderedTrie<TValue>
    {
        private List<KeyValuePair<string, TValue>> list = new List<KeyValuePair<string, TValue>>();
        private TrieNode<TValue> node = new TrieNode<TValue>();

        public int Count => list.Count;
        public KeyValuePair<string, TValue> this[int index] => list[index];

        public TrieContains Contains(string key) => node.Contains(key);

        public void Add(string key, TValue value) => Insert(list.Count, key, value);
        public void Insert(int index, string key, TValue value)
        {
            list.Insert(index, new KeyValuePair<string, TValue>(key, value));
            node.Insert(key, value);
        }
    }
}

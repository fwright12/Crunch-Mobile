using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Crunch
{
    /// <summary>
    /// A dictionary structure with multiple dictionaries separated by key type
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    public class GroupedDictionary<TValue>
    {
        private Dictionary<Type, Dictionary<object, TValue>> data = new Dictionary<Type, Dictionary<object, TValue>>();

        public int Count
        {
            get
            {
                int count = 0;
                foreach (Type t in data.Keys)
                {
                    count += data[t].Count;
                }
                return count;
            }
        }

        public TValue this[object key]
        {
            get { return data.TryGet(key.GetType())[key]; }
            set { data.TryGet(key.GetType())[key] = value; }
        }

        public int TypeCount(Type type) => data.ContainsKey(type) ? data[type].Count : 0;
        public void Add(object key, TValue value) => data.TryGet(key.GetType()).Add(key, value);
        public void Remove(object key) => data.TryGet(key.GetType()).Remove(key);
        public void RemoveType(Type type) => data.Remove(type);
        public IEnumerable EnumerateKeys<T>()
        {
            if (data.ContainsKey(typeof(T)))
            {
                foreach (object o in data[typeof(T)].Keys)
                {
                    yield return (T)o;
                }
            }
        }

        public bool ContainsKey(object key) { return data.TryGet(key.GetType()).ContainsKey(key); }

        public IEnumerable KeyValuePairs()// IEnumerable.GetEnumerator()
        {
            foreach(Type t in data.Keys)
            {
                foreach(object o in data[t].Keys)
                {
                    yield return new KeyValuePair<object, TValue>(o, data[t][o]);
                }
            }
        }
    }
}

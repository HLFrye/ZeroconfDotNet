using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZeroconfDotNet.DNS
{
    public partial class ServiceWatchManager
    {
        class TTLDict<T> : IDictionary<string, T> where T : ITTL
        {
            private readonly IDictionary<string, T> _base;
            public TTLDict()
                : this(new Dictionary<string, T>())
            { }

            public TTLDict(IDictionary<string, T> baseDict)
            {
                _base = baseDict;
            }

            DateTime _lastClean = DateTime.Now;
            TimeSpan refreshTime = TimeSpan.FromSeconds(5);
            void Cleanup()
            {
                if (DateTime.Now - _lastClean < refreshTime)
                    return;
                _lastClean = DateTime.Now;

                var toRemove = _base.Where(data => data.Value.ExpireAt < DateTime.Now).Select(data => data.Key).ToList();
                foreach (var key in toRemove)
                {
                    _base.Remove(key);
                }
            }

            public void Add(string key, T value)
            {
                _base.Add(key, value);
            }

            public bool ContainsKey(string key)
            {
                Cleanup();
                return _base.ContainsKey(key);
            }

            public ICollection<string> Keys
            {
                get
                {
                    Cleanup();
                    return _base.Keys;
                }
            }

            public bool Remove(string key)
            {
                return _base.Remove(key);
            }

            public bool TryGetValue(string key, out T value)
            {
                Cleanup();
                return _base.TryGetValue(key, out value);
            }

            public ICollection<T> Values
            {
                get { Cleanup(); return _base.Values; }
            }

            public T this[string key]
            {
                get
                {
                    Cleanup();
                    return _base[key];
                }
                set
                {
                    _base[key] = value;
                }
            }

            public void Add(KeyValuePair<string, T> item)
            {
                _base.Add(item);
            }

            public void Clear()
            {
                _base.Clear();
            }

            public bool Contains(KeyValuePair<string, T> item)
            {
                Cleanup();
                return _base.Contains(item);

            }

            public void CopyTo(KeyValuePair<string, T>[] array, int arrayIndex)
            {
                Cleanup();
                _base.CopyTo(array, arrayIndex);
            }

            public int Count
            {
                get { Cleanup(); return _base.Count; }
            }

            public bool IsReadOnly
            {
                get { return _base.IsReadOnly; }
            }

            public bool Remove(KeyValuePair<string, T> item)
            {
                return _base.Remove(item);
            }

            public IEnumerator<KeyValuePair<string, T>> GetEnumerator()
            {
                Cleanup();
                return _base.GetEnumerator();
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                Cleanup();
                return _base.GetEnumerator();
            }
        }
    }
}

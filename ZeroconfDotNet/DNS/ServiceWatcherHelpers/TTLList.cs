using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DiscoveryDotNet.DNS
{
    public partial class ServiceWatchManager
    {
        class TTLList<T> : IList<T> where T : ITTL
        {
            public TTLList()
                : this(new List<T>())
            {
            }

            private readonly IList<T> _base;
            public TTLList(IList<T> _baseList)
            {
                _base = _baseList;
            }

            void Cleanup()
            {
                var toRemove = _base.Where(x => x.ExpireAt < DateTime.Now).ToList();
                foreach (var item in toRemove)
                {
                    _base.Remove(item);
                }
            }

            public int IndexOf(T item)
            {
                Cleanup();
                return _base.IndexOf(item);
            }

            public void Insert(int index, T item)
            {
                _base.Insert(index, item);
            }

            public void RemoveAt(int index)
            {
                _base.RemoveAt(index);
            }

            public T this[int index]
            {
                get
                {
                    Cleanup();
                    return _base[index];
                }
                set
                {
                    _base[index] = value;
                }
            }

            public void Add(T item)
            {
                _base.Add(item);
            }

            public void Clear()
            {
                _base.Clear();
            }

            public bool Contains(T item)
            {
                Cleanup();
                return _base.Contains(item);
            }

            public void CopyTo(T[] array, int arrayIndex)
            {
                Cleanup();
                _base.CopyTo(array, arrayIndex);
            }

            public int Count
            {
                get
                {
                    Cleanup();
                    return _base.Count;
                }
            }

            public bool IsReadOnly
            {
                get { return _base.IsReadOnly; }
            }

            public bool Remove(T item)
            {
                return _base.Remove(item);
            }

            public IEnumerator<T> GetEnumerator()
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

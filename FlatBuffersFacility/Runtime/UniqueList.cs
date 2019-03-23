using System.Collections;
using System.Collections.Generic;

namespace FlatBuffersFacility
{
    public class UniqueList<T> : IList<T> 
    {
        private HashSet<T> set;
        private List<T> list;

        public UniqueList()
        {
            list = new List<T>();
            set = new HashSet<T>();
        }

        public void Add(T item)
        {
            if (set.Contains(item))
            {
                return;
            }

            list.Add(item);
            set.Add(item);
        }

        public void Clear()
        {
            list.Clear();
            set.Clear();
        }

        public bool Contains(T item)
        {
            return set.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            list.CopyTo(array, arrayIndex);
            set.CopyTo(array, arrayIndex);
        }

        public bool Remove(T item)
        {
            if (set.Contains(item))
            {
                set.Remove(item);
                list.Remove(item);
                return true;
            }

            return false;
        }

        public int Count
        {
            get { return list.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public IEnumerator<T> GetEnumerator()
        {
            return list.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int IndexOf(T item)
        {
            return list.IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            if (set.Contains(item))
            {
                return;
            }

            list.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            list.RemoveAt(index);
        }

        public T this[int index]
        {
            get { return list[index]; }
            set { list[index] = value; }
        }

        public T PopLast()
        {
            if (list.Count == 0)
            {
                return default(T);
            }

            int lastIndex = list.Count - 1;
            T result = list[lastIndex];
            list.RemoveAt(lastIndex);
            return result;
        }
    }
}
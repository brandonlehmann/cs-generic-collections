using System.Threading;

namespace Generic.Collections
{
    public class ArrayList<T>
    {
        private static Mutex mutex = new Mutex();
        private System.Collections.ArrayList array = new System.Collections.ArrayList();

        public ArrayList()
        {
            array = new System.Collections.ArrayList();
        }
        public ArrayList(System.Collections.ICollection c)
        {
            array = new System.Collections.ArrayList(c);
        }
        public ArrayList(int capacity)
        {
            array = new System.Collections.ArrayList(capacity);
        }
        public void Add(T obj)
        {
            mutex.WaitOne();
            try
            {
                array.Add(obj);
            }
            finally
            {
                mutex.ReleaseMutex();
            }
        }
        public void Clear()
        {
            mutex.WaitOne();
            try
            {
                array.Clear();
            }
            finally
            {
                mutex.ReleaseMutex();
            }
        }
        public int Count
        {
            get
            {
                mutex.WaitOne();
                try
                {
                    return array.Count;
                }
                finally
                {
                    mutex.ReleaseMutex();
                }
            }
        }
        public int Capacity
        {
            get
            {
                mutex.WaitOne();
                try
                {
                    return array.Count;
                }
                finally
                {
                    mutex.ReleaseMutex();
                }
            }
            set
            {
                mutex.WaitOne();
                try
                {
                    array.Capacity = value;
                }
                finally
                {
                    mutex.ReleaseMutex();
                }
            }
        }
        public void Remove(T obj)
        {
            mutex.WaitOne();
            try
            {
                array.Remove(obj);
            }
            finally
            {
                mutex.ReleaseMutex();
            }
        }
        public void RemoveAt(int index)
        {
            mutex.WaitOne();
            try
            {
                array.RemoveAt(index);
            }
            finally
            {
                mutex.ReleaseMutex();
            }
        }
        public void TrimToSize()
        {
            mutex.WaitOne();
            try
            {
                array.TrimToSize();
            }
            finally
            {
                mutex.ReleaseMutex();
            }
        }
        public bool Contains(T obj)
        {
            mutex.WaitOne();
            try
            {
                return array.Contains(obj);
            }
            finally
            {
                mutex.ReleaseMutex();
            }
        }
        public System.Collections.IEnumerator GetEnumerator()
        {
            mutex.WaitOne();
            try
            {
                return array.GetEnumerator();
            }
            finally
            {
                mutex.ReleaseMutex();
            }
        }
        /*public void Insert(int index, T obj)
        {
            mutex.WaitOne();
            try
            {
                array.Insert(index, T);
            }
            finally
            {
                mutex.ReleaseMutex();
            }
        }*/
        public System.Collections.IEnumerator GetEnumerator(int index, int count)
        {
            mutex.WaitOne();
            try
            {
                return array.GetEnumerator(index, count);
            }
            finally
            {
                mutex.ReleaseMutex();
            }
        }
        public int IndexOf(T obj)
        {
            mutex.WaitOne();
            try
            {
                return array.IndexOf(obj);
            }
            finally
            {
                mutex.ReleaseMutex();
            }
        }
        public int LastIndexOf(T obj)
        {
            mutex.WaitOne();
            try
            {
                return array.LastIndexOf(obj);
            }
            finally
            {
                mutex.ReleaseMutex();
            }
        }
        public void Reverse()
        {
            mutex.WaitOne();
            try
            {
                array.Reverse();
            }
            finally
            {
                mutex.ReleaseMutex();
            }
        }
        public void Sort()
        {
            mutex.WaitOne();
            try
            {
                array.Sort();
            }
            finally
            {
                mutex.ReleaseMutex();
            }
        }
        public T this[int index]
        {
            get
            {
                mutex.WaitOne();
                try
                {
                    return (T)array[index];
                }
                catch {
                    return default(T);
                }
                finally
                {
                    mutex.ReleaseMutex();
                }
            }
            set
            {
                mutex.WaitOne();
                try
                {
                    array[index] = value;
                }
                catch { }
                finally
                {
                    mutex.ReleaseMutex();
                }
            }
        }
    }
}

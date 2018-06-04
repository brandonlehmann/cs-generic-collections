using System.Threading;

namespace Generic.Collections
{
    public class Dictionary<TKey,TValue>
    {
        private System.Collections.Generic.Dictionary<TKey, TValue> dictionary;
        private static Mutex mutex = new Mutex();

        public Dictionary()
        {
            dictionary = new System.Collections.Generic.Dictionary<TKey, TValue>();
        }
        public Dictionary(int capacity)
        {
            dictionary = new System.Collections.Generic.Dictionary<TKey, TValue>(capacity);
        }
        public void Add(TKey key, TValue value)
        {
            if (ContainsKey(key))
                Remove(key);
            mutex.WaitOne();
            try
            {
                dictionary.Add(key, value);
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
                dictionary.Clear();
            }
            finally
            {
                mutex.ReleaseMutex();
            }
        }
        public bool ContainsKey(TKey key)
        {
            mutex.WaitOne();
            try
            {
                return dictionary.ContainsKey(key);
            }
            finally
            {
                mutex.ReleaseMutex();
            }
        }
        public bool ContainsValue(TValue value)
        {
            mutex.WaitOne();
            try
            {
                return dictionary.ContainsValue(value);
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
                    return dictionary.Count;
                }
                finally
                {
                    mutex.ReleaseMutex();
                }
            }
        }
        public System.Collections.Generic.Dictionary<TKey, TValue>.Enumerator GetEnumerator()
        {
            mutex.WaitOne();
            try
            {
                return dictionary.GetEnumerator();
            }
            finally
            {
                mutex.ReleaseMutex();
            }
        }
        public System.Collections.Generic.Dictionary<TKey, TValue>.KeyCollection Keys
        {
            get
            {
                mutex.WaitOne();
                try
                {
                    return dictionary.Keys;
                }
                finally
                {
                    mutex.ReleaseMutex();
                }
            }
        }
        public System.Collections.Generic.Dictionary<TKey, TValue>.ValueCollection Values
        {
            get
            {
                mutex.WaitOne();
                try
                {
                    return dictionary.Values;
                }
                finally
                {
                    mutex.ReleaseMutex();
                }
            }
        }
        public TValue this[TKey key]
        {
            get
            {
                mutex.WaitOne();
                try
                {
                    return dictionary[key];
                }
                catch
                {
                    return default(TValue);
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
                    dictionary[key] = value;
                }
                finally
                {
                    mutex.ReleaseMutex();
                }
            }
        }
        public void Remove(TKey key)
        {
            mutex.WaitOne();
            try
            {
                dictionary.Remove(key);
            }
            finally
            {
                mutex.ReleaseMutex();
            }
        }
    }
}

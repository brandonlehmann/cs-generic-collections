using System;
using System.Collections;
using System.Threading;
using System.Text;

namespace Generic.Collections
{
    public class QueueList<T>
    {
        private static Mutex mutex = new Mutex();
        private System.Collections.ArrayList arraylist;

        public QueueList()
        {
            arraylist = new System.Collections.ArrayList();
        }
        public QueueList(int capacity)
        {
            arraylist = new System.Collections.ArrayList(capacity);
        }
        public void Enqueue(T obj, int index)
        {
            mutex.WaitOne();
            try
            {
                if (index > arraylist.Count)
                {
                    arraylist.Add(obj);
                }
                else if (index != -1)
                {
                    arraylist.Insert(index, obj);
                }
                else
                {
                    arraylist.Add(obj);
                }
            }
            finally
            {
                mutex.ReleaseMutex();
            }
        }
        public void Enqueue(T obj)
        {
            Enqueue(obj, -1);
        }
        public void Enqueue(T obj, bool top)
        {
            if (top)
            {
                Enqueue(obj, 0);
            }
            else
            {
                Enqueue(obj);
            }
        }
        public T Dequeue()
        {
            mutex.WaitOne();
            T item = default(T);
            try
            {
                item = (T)arraylist[0];
                arraylist.RemoveAt(0);
            }
            finally
            {
                mutex.ReleaseMutex();
            }
            return item;
        }
        public int Count
        {
            get
            {
                mutex.WaitOne();
                int count = 0;
                try
                {
                    count = arraylist.Count;
                }
                finally
                {
                    mutex.ReleaseMutex();
                }
                return count;
            }
        }
        public T Peek()
        {
            mutex.WaitOne();
            T item = default(T);
            try
            {
                item = (T)arraylist[0];
            }
            finally
            {
                mutex.ReleaseMutex();
            }
            return item;
        }
        public void Clear()
        {
            mutex.WaitOne();
            try
            {
                arraylist.Clear();
            }
            finally
            {
                mutex.ReleaseMutex();
            }
        }
    }
}

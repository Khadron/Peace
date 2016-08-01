using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace Peace.LRU
{
    public interface ICache<K, V> where K : IComparable
    {
        V Get(K key);//

        void Put(K key, V value);
        void Put(K key, V value, long validTime);

        void Remove(K key);

        Pair<K, V>[] GetAll();

        int Size();
    }

    public class Pair<K, V> : IComparable where K : IComparable
    {
        public K Key { get; set; }

        public V Value { get; set; }

        public Pair(K key, V value)
        {
            this.Key = key;
            this.Value = value;
        }

        public int CompareTo(object obj)
        {
            if (obj is Pair<K, V>)
            {
                Pair<K, V> pair = obj as Pair<K, V>;
                int result = this.Key.CompareTo(pair.Key);

                if (result == 0)
                {
                    if ((this.Value is IComparable) && (pair.Value is IComparable))
                    {
                        return (this.Value as IComparable).CompareTo(pair.Value);
                    }
                }
            }

            return -1;
        }

        public override bool Equals(object obj)
        {
            if (obj is Pair<K, V>)
            {
                Pair<K, V> pair = obj as Pair<K, V>;
                return this.Key.Equals(pair.Key) && this.Value.Equals(pair.Value);
            }
            return false;
        }

        public override int GetHashCode()
        {
            return Key.GetHashCode() ^ Value.GetHashCode();
        }

        public override string ToString()
        {
            return this.Key + ": " + this.Value;
        }
    }

    public class LRUCache<K, V> : ICache<K, V> where K : IComparable
    {
        private const int FixCount = 100000;
        private static object _objectLock = new object();
        private readonly Hashtable _table;
        private readonly Item _head;
        private readonly Item _end;
        private readonly long _maxSize;

        public LRUCache()
            : this(FixCount)
        {
        }

        public LRUCache(long maxSize)
        {
            _table = Hashtable.Synchronized(new Hashtable());
            _head = new Item();
            _end = new Item();
            _maxSize = maxSize;
        }

        public long MaxSize
        {
            get { return _maxSize; }
        }

        public V Get(K key)
        {
            Item curItem = _table[key] as Item;

            if (curItem == null)
            {
                return default(V);
            }

            if (CacheUtils.CurrentTimeMillis() > curItem.Expires)
            {
                _table.Remove(key);

                RemoveItem(curItem);
                return default(V);
            }

            //
            if (curItem != _head.Next)
            {

                MoveToHead(curItem);
            }

            return (V)curItem.Value;
        }

        public void Put(K key, V value)
        {
            Put(key, value, -1);
        }

        public void Put(K key, V value, long validTime)
        {
            Item curItem = _table[key] as Item;

            if (curItem != null)
            {
                curItem.Value = value;

                if (validTime > 0)
                {
                    curItem.Expires = CacheUtils.CurrentTimeMillis() + validTime;
                }
                else
                {
                    curItem.Expires = long.MaxValue;
                }

                MoveToHead(curItem);
                return;
            }

            if (_table.Count >= _maxSize)
            {
                curItem = _end.Previous;
                _table.Remove(curItem.Key);

                RemoveItem(curItem);
            }

            long expires = validTime > 0 ? CacheUtils.CurrentTimeMillis() + validTime : long.MaxValue;

            curItem = new Item(key, value, expires);
            InsertHead(curItem);
            curItem.Next = _end;
            _table.Add(key, curItem);
        }

        public void Remove(K key)
        {
            Item curItem = _table[key] as Item;
            if (curItem != null)
            {
                _table.Remove(key);
                RemoveItem(curItem);
            }
        }

        public Pair<K, V>[] GetAll()
        {
            Pair<K, V>[] pairs = new Pair<K, V>[_maxSize];

            lock (_objectLock)
            {
                Item curItem = _head.Next;
                int index = 0;
                while (curItem != _end)
                {
                    pairs[index] = new Pair<K, V>((K)curItem.Key, (V)curItem.Value);
                    ++index;
                    curItem = curItem.Next;
                }

            }

            return pairs;
        }

        public int Size()
        {
            return _table.Count;
        }

        protected class Item
        {
            public Item()
            {
            }

            public Item(IComparable k, object v, long e)
            {
                Key = k;
                Value = v;
                Expires = e;
            }

            public IComparable Key { get; set; }

            public Object Value { get; set; }

            public long Expires { get; set; }

            public Item Previous { get; set; }

            public Item Next { get; set; }
        }

        private void MoveToHead(Item curItem)
        {
            lock (_objectLock)
            {
                curItem.Previous.Next = curItem.Next;

                curItem.Next.Previous = curItem.Previous;

                curItem.Previous = _head;
                curItem.Next = _head.Next;

                _head.Next.Previous = curItem;
                _head.Next = curItem;
            }
        }
        private void InsertHead(Item curItem)
        {
            lock (_objectLock)
            {
                curItem.Previous = _head;
                curItem.Next = _head.Next;

                _head.Next = curItem;
                _head.Next.Previous = curItem;
            }
        }
        private static void RemoveItem(Item curItem)
        {
            lock (_objectLock)
            {
                curItem.Previous.Next = curItem.Next;
                curItem.Next.Previous = curItem.Previous;
            }
        }

    }

    public static class CacheUtils
    {
        public static long CurrentTimeMillis()
        {
            return (long)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
        }

    }

}

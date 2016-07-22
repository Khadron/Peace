using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Peace.IoC
{
    public class Cache<TKey, TValue>
    {
        private readonly Dictionary<TKey, TValue> _dictionary = new Dictionary<TKey, TValue>();

        private readonly ReaderWriterLock _rwLock = new ReaderWriterLock();

        private readonly TimeSpan _lockTimeOut = TimeSpan.FromMilliseconds(100);

        #region Methods

        public void Add(TKey key, TValue value)
        {
            bool isExisting = false;
            _rwLock.AcquireWriterLock(_lockTimeOut);
            try
            {
                if (!_dictionary.ContainsKey(key))
                    _dictionary.Add(key, value);
                else
                    isExisting = true;
            }
            finally { _rwLock.ReleaseWriterLock(); }
            if (isExisting) throw new IndexOutOfRangeException();
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            _rwLock.AcquireReaderLock(_lockTimeOut);
            bool result;
            try
            {
                result = _dictionary.TryGetValue(key, out value);
            }
            finally { _rwLock.ReleaseReaderLock(); }
            return result;
        }

        public void Clear()
        {
            if (_dictionary.Count > 0)
            {
                _rwLock.AcquireWriterLock(_lockTimeOut);
                try
                {
                    _dictionary.Clear();
                }
                finally { _rwLock.ReleaseWriterLock(); }
            }
        }

        public bool ContainsKey(TKey key)
        {
            if (_dictionary.Count <= 0) return false;
            bool result;
            _rwLock.AcquireReaderLock(_lockTimeOut);
            try
            {
                result = _dictionary.ContainsKey(key);
            }
            finally { _rwLock.ReleaseReaderLock(); }
            return result;
        }

        #endregion

        #region Properties

        public int Count
        {
            get { return _dictionary.Count; }
        }

        public TValue this[TKey key]
        {

            get
            {
                TValue outVal;
                if (TryGetValue(key, out outVal))
                {
                    return outVal;
                }

                return default(TValue);
            }
        }

        public IList<TValue> Values
        {
            get { return _dictionary.Values.ToList(); }
        }

        #endregion


    }
}

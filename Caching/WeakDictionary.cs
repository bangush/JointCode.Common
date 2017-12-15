//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.Diagnostics.CodeAnalysis;
//using System.Runtime.InteropServices;
//using JointCode.Common.Threading;

//namespace JointCode.Common.Caching
//{
//    /// <summary>
//    /// A weak dictionary that keep weak references for values and allow them to be collected.
//    /// </summary>
//    /// <typeparam name="TLock">A derivation of <see cref="IReaderWriterLockSlim"/>, i.e, <see cref="SpinReaderWriterLockSlim"/> or <see cref="OptimisticReaderWriterLock"/>, depends on how many cores the CPU has.</typeparam>
//    /// <typeparam name="TKey">The type of the key.</typeparam>
//    /// <typeparam name="TValue">The type of the value.</typeparam>
//    /// <seealso cref="JointCode.Common.ThreadSafeCriticalDisposable" />
//    /// <seealso cref="System.Collections.Generic.IDictionary{TKey, TValue}" />
//    /// <seealso cref="IGcObserver" />
//    public sealed class WeakDictionary<TLock, TKey, TValue> :
//        ThreadSafeCriticalDisposable,
//        IDictionary<TKey, TValue>,
//        //ISerializable,
//        IGcObserver
//        where TValue : class
//        where TLock : IReaderWriterLockSlim
//    {
//        Dictionary<TKey, GCHandle> _dictionary = new Dictionary<TKey, GCHandle>();
//        TLock _syncObj;

//        /// <summary>
//        /// Creates the dictionary.
//        /// </summary>
//        public WeakDictionary(TLock syncObj)
//        {
//            _syncObj = syncObj;
//            GcUtils.RegisterGcObserver(this);
//        }

//        #region Dispose

//        /// <summary>
//        /// Frees all handles used to know if an item was collected or not.
//        /// </summary>
//        protected override void Dispose(bool disposing)
//        {
//            GcUtils.UnregisterGcObserver(this);
//            var dictionary = _dictionary;
//            if (dictionary != null)
//            {
//                _dictionary = null;
//                foreach (GCHandle handle in dictionary.Values)
//                    handle.Free();
//            }
//            base.Dispose(disposing);
//        }

//        #endregion

//        #region OnGcCollected

//        void IGcObserver.OnGarbageCollected()
//        {
//            _syncObj.EnterWriteLock();
//            try
//            {
//                DoCollect();
//            }
//            finally
//            {
//                _syncObj.EnterWriteLock();
//            }
//        }

//        void DoCollect()
//        {
//            if (Disposed)
//            {
//                GcUtils.UnregisterGcObserver(this);
//                return;
//            }

//            var oldDictionary = _dictionary;
//            var newDictionary = new Dictionary<TKey, GCHandle>(oldDictionary.Count);
//            foreach (var pair in oldDictionary)
//            {
//                var handle = pair.Value;
//                if (handle.Target != null)
//                    newDictionary.Add(pair.Key, pair.Value);
//                else
//                    handle.Free();
//            }

//            _dictionary = newDictionary;
//        }

//        #endregion

//        #region Properties

//        /// <summary>
//        /// Gets the number of items in this dictionary.
//        /// </summary>
//        public int Count
//        {
//            get { return _dictionary.Count; }
//        }

//        /// <summary>
//        /// Gets or sets a value for the specified key.
//        /// Returns null if the item does not exist. The indexer, when
//        /// used as an IDictionary throws an exception when the item does
//        /// not exist.
//        /// </summary>
//        public TValue this[TKey key]
//        {
//            get
//            {
//                if (key == null) throw new ArgumentNullException("key");
//                try
//                {
//                    AssertNotDisposed();
//                    GCHandle handle;
//                    if (_dictionary.TryGetValue(key, out handle))
//                        return (TValue)handle.Target;
//                }
//                finally
//                {
//                    _syncObj.ExitReadLock();
//                }
//                return default(TValue);
//            }
//            set
//            {
//                if (key == null) throw new ArgumentNullException("key");
//                _syncObj.EnterWriteLock();
//                try
//                {
//                    AssertNotDisposed();
//                    if (value == null)
//                    {
//                        _Remove(key);
//                    }
//                    else
//                    {
//                        GCHandle handle;
//                        var dictionary = _dictionary;
//                        if (dictionary.TryGetValue(key, out handle))
//                            handle.Target = value;
//                        else
//                            _Add(key, value, dictionary);
//                    }
//                }
//                finally
//                {
//                    _syncObj.EnterWriteLock();
//                }
//            }
//        }

//        /// <summary>
//        /// Gets the Keys that exist in this dictionary.
//        /// </summary>
//        public ICollection<TKey> Keys
//        {
//            get
//            {
//                try
//                {
//                    AssertNotDisposed();
//                    return _dictionary.Keys;
//                }
//                finally
//                {
//                    _syncObj.ExitReadLock();
//                }
//            }
//        }

//        /// <summary>
//        /// Gets a copy of the values that exist in this dictionary.
//        /// </summary>
//        public ICollection<TValue> Values
//        {
//            get
//            {
//                try
//                {
//                    AssertNotDisposed();
//                    var dictionary = _dictionary;
//                    List<TValue> result = new List<TValue>();
//                    foreach (GCHandle handle in dictionary.Values)
//                    {
//                        TValue item = (TValue)handle.Target;
//                        if (item != null)
//                            result.Add(item);
//                    }
//                    return result;
//                }
//                finally
//                {
//                    _syncObj.ExitReadLock();
//                }
//            }
//        }

//        #endregion

//        #region Methods

//        /// <summary>
//        /// Clears all items in this dictionary.
//        /// </summary>
//        public void Clear()
//        {
//            _syncObj.EnterWriteLock();
//            try
//            {
//                AssertNotDisposed();
//                var dictionary = _dictionary;
//                try
//                {
//                }
//                finally
//                {
//                    foreach (GCHandle handle in dictionary.Values)
//                        handle.Free();
//                    dictionary.Clear();
//                }
//            }
//            finally
//            {
//                _syncObj.EnterWriteLock();
//            }
//        }

//        /// <summary>
//        /// Adds an item to this dictionary. Throws an exception if an item
//        /// with the same key already exists.
//        /// </summary>
//        public void Add(TKey key, TValue value)
//        {
//            if (key == null) throw new ArgumentNullException("key");
//            if (value == null) throw new ArgumentNullException("value");

//            _syncObj.EnterWriteLock();
//            try
//            {
//                AssertNotDisposed();
//                var dictionary = _dictionary;
//                GCHandle handle;
//                if (dictionary.TryGetValue(key, out handle))
//                {
//                    if (handle.Target != null)
//                        throw new ArgumentException("An element with the same key \"" + key + "\" already exists.");
//                    handle.Target = value;
//                }
//                else
//                {
//                    _Add(key, value, dictionary);
//                }
//            }
//            finally
//            {
//                _syncObj.EnterWriteLock();
//            }
//        }

//        /// <summary>
//        /// Removes an item with the given key from the dictionary.
//        /// </summary>
//        public bool Remove(TKey key)
//        {
//            if (key == null) throw new ArgumentNullException("key");
//            _syncObj.EnterWriteLock();
//            try
//            {
//                AssertNotDisposed();
//                return _Remove(key);
//            }
//            finally
//            {
//                _syncObj.EnterWriteLock();
//            }
//        }

//        /// <summary>
//        /// Gets a value indicating if an item with the specified key exists.
//        /// </summary>
//        public bool ContainsKey(TKey key)
//        {
//            return _dictionary.ContainsKey(key);
//        }

//        #region GetEnumerator
//        /// <summary>
//        /// Gets an enumerator with all key/value pairs that exist in
//        /// this dictionary.
//        /// </summary>
//        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
//        {
//            return ToList().GetEnumerator();
//        }
//        IEnumerator IEnumerable.GetEnumerator()
//        {
//            return GetEnumerator();
//        }
//        #endregion

//        /// <summary>
//        /// Gets a list with all non-collected key/value pairs.
//        /// </summary>
//        public List<KeyValuePair<TKey, TValue>> ToList()
//        {
//            try
//            {
//                AssertNotDisposed();
//                var dictionary = _dictionary;
//                var result = new List<KeyValuePair<TKey, TValue>>(dictionary.Count);
//                foreach (var pair in dictionary)
//                {
//                    TValue target = (TValue)pair.Value.Target;
//                    if (target != null)
//                        result.Add(new KeyValuePair<TKey, TValue>(pair.Key, target));
//                }
//                return result;
//            }
//            finally
//            {
//                _syncObj.ExitReadLock();
//            }
//        }

//        [SuppressMessage("Microsoft.Usage", "CA2219:DoNotRaiseExceptionsInExceptionClauses")]
//        static void _Add(TKey key, TValue value, Dictionary<TKey, GCHandle> dictionary)
//        {
//            try
//            {
//            }
//            finally
//            {
//                GCHandle handle = GCHandle.Alloc(value, GCHandleType.Weak);
//                try
//                {
//                    dictionary.Add(key, handle);
//                }
//                catch
//                {
//                    handle.Free();
//                    throw;
//                }
//            }
//        }

//        bool _Remove(TKey key)
//        {
//            var dictionary = _dictionary;
//            GCHandle handle;
//            if (!dictionary.TryGetValue(key, out handle))
//                return false;

//            bool result;
//            try
//            {
//            }
//            finally
//            {
//                handle.Free();
//                result = dictionary.Remove(key);
//            }
//            return result;
//        }

//        #endregion

//        #region IDictionary<TKey,TValue> Members
//        bool IDictionary<TKey, TValue>.TryGetValue(TKey key, out TValue value)
//        {
//            value = this[key];
//            return value != null;
//        }
//        TValue IDictionary<TKey, TValue>.this[TKey key]
//        {
//            get
//            {
//                TValue result = this[key];
//                if (result == null) throw new KeyNotFoundException("The given key \"" + key + "\" was not found in the dictionary.");
//                return result;
//            }
//            set
//            {
//                this[key] = value;
//            }
//        }
//        #endregion

//        #region ICollection<KeyValuePair<TKey,TValue>> Members
//        void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item)
//        {
//            Add(item.Key, item.Value);
//        }
//        bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> item)
//        {
//            if (item.Value == null) return false;
//            GCHandle handle;
//            if (!_dictionary.TryGetValue(item.Key, out handle))
//                return false;
//            return object.Equals(handle.Target, item.Value);
//        }
//        void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
//        {
//            ToList().CopyTo(array, arrayIndex);
//        }
//        bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly
//        {
//            get { return false; }
//        }
//        bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item)
//        {
//            if (item.Value == null)
//                return false;

//            bool upgraded = false;
//            _syncObj.EnterUpgradeableLock();
//            try
//            {
//                AssertNotDisposed();
//                GCHandle handle;
//                var dictionary = _dictionary;
//                if (!dictionary.TryGetValue(item.Key, out handle))
//                    return false;
//                if (!object.Equals(handle.Target, item.Value))
//                    return false;

//                _syncObj.UpgradeToWriteLock(ref upgraded);
//                return _Remove(item.Key);
//            }
//            finally
//            {
//                _syncObj.ExitUpgradeableLock(upgraded);
//            }
//        }
//        #endregion

//        //#region ISerializable Members
//        ///// <summary>
//        ///// Creates the dictionary from serialization info.
//        ///// Actually, it does not load anything, as if everything was
//        ///// collected.
//        ///// </summary>
//        //internal WeakDictionary(SerializationInfo info, StreamingContext context) :
//        //    this()
//        //{
//        //}
//        //void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
//        //{
//        //}
//        //#endregion
//    }
//}

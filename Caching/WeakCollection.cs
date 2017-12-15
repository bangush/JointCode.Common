//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.Runtime.InteropServices;
//using JointCode.Common.Threading;

//namespace JointCode.Common.Caching
//{
//    /// <summary>
//    /// A collection that keep weak references to its items and allow them to be collected.
//    /// </summary>
//    /// <typeparam name="TLock">A derivation of <see cref="IReaderWriterLockSlim"/>, i.e, <see cref="SpinReaderWriterLockSlim"/> or <see cref="OptimisticReaderWriterLock"/>, depends on how many cores the CPU has.</typeparam>
//    /// <typeparam name="T"></typeparam>
//    /// <seealso cref="JointCode.Common.ThreadSafeCriticalDisposable" />
//    /// <seealso cref="System.Collections.Generic.ICollection{T}" />
//    /// <seealso cref="IGcObserver" />
//    public class WeakCollection<TLock, T> :
//        ThreadSafeCriticalDisposable,
//        //ISerializable,
//        ICollection<T>,
//        IGcObserver
//        where T : class
//        where TLock : IReaderWriterLockSlim
//    {
//        static readonly Comparison<GCHandle> _sortHandles = _SortHandles;
//        GCHandle[] _handles;
//        TLock _syncObj;

//        /// <summary>
//        /// Creates an empty weak-list.
//        /// </summary>
//        public WeakCollection(TLock syncObj) : this(32, syncObj) { }

//        /// <summary>
//        /// Creates an empty weak-list using the given minCapacity to it.
//        /// </summary>
//        /// <param name="initialCapacity">The initialCapacity of the list. The default value is 32.</param>
//        /// <param name="syncObj">The synchronize object.</param>
//        /// <exception cref="ArgumentOutOfRangeException">initialCapacity;The initial capacity value can not be less than 1!</exception>
//        public WeakCollection(int initialCapacity, TLock syncObj)
//        {
//            if (initialCapacity < 1)
//                throw new ArgumentOutOfRangeException("initialCapacity", "The initial capacity value can not be less than 1!");
//            _handles = new GCHandle[initialCapacity];
//            _syncObj = syncObj;
//            GcUtils.RegisterGcObserver(this);
//        }

//        #region Dispose

//        /// <summary>
//        /// Releases all handles.
//        /// </summary>
//        protected override void Dispose(bool disposing)
//        {
//            GcUtils.UnregisterGcObserver(this);
//            // here we will free all allocated handles. After all, even if the objects can be collected, the
//            // handles must be freed.
//            var handles = _handles;
//            if (handles != null)
//            {
//                int count = handles.Length;
//                for (int i = 0; i < count; i++)
//                {
//                    var handle = handles[i];
//                    if (handle.IsAllocated)
//                        handle.Free();
//                }
//                _handles = null;
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
//                _syncObj.ExitWriteLock();
//            }
//        }

//        void DoCollect()
//        {
//            if (Disposed)
//            {
//                GcUtils.UnregisterGcObserver(this);
//                return;
//            }

//            int allocated = 0;
//            int count = Count;
//            for (int i = 0; i < count; i++)
//            {
//                var handle = _handles[i];
//                if (handle.IsAllocated && handle.Target != null)
//                    allocated++;
//            }

//            int minCapacity = count / 2;
//            if (minCapacity < 32)
//                minCapacity = 32;

//            if (allocated < minCapacity)
//                allocated = minCapacity;

//            if (allocated != _handles.Length)
//            {
//                var newHandles = new GCHandle[allocated];
//                try
//                {
//                }
//                finally
//                {
//                    int newIndex = 0;
//                    for (int i = 0; i < count; i++)
//                    {
//                        var handle = _handles[i];
//                        if (!handle.IsAllocated)
//                            continue;

//                        var target = handle.Target;
//                        if (target == null)
//                        {
//                            handle.Free();
//                        }
//                        else
//                        {
//                            newHandles[newIndex] = handle;
//                            newIndex++;
//                        }
//                    }
//                    for (int i = count; i < _handles.Length; i++)
//                    {
//                        var handle = _handles[i];
//                        if (handle.IsAllocated)
//                            handle.Free();
//                    }

//                    Count = newIndex;
//                    _handles = newHandles;
//                }
//            }
//        }

//        #endregion

//        /// <summary>
//        /// Gets an approximate count of the items added.
//        /// </summary>
//        public int Count { get; set; }

//        /// <summary>
//        /// Adds an item to the list.
//        /// </summary>
//        public void Add(T item)
//        {
//            if (item == null) throw new ArgumentNullException("item");
//            _syncObj.EnterWriteLock();
//            try
//            {
//                DoAdd(item);
//            }
//            finally
//            {
//                _syncObj.ExitWriteLock();
//            }
//        }

//        void DoAdd(T item)
//        {
//            AssertNotDisposed();
//            int count = Count;
//            if (count == _handles.Length)
//            {
//                bool mustReturn = false;
//                try
//                {
//                }
//                finally
//                {
//                    Array.Sort(_handles, _sortHandles);
//                    for (int i = 0; i < count; i++)
//                    {
//                        var handle1 = _handles[i];
//                        if (handle1.Target == null)
//                        {
//                            handle1.Target = item;
//                            Count = i + 1;
//                            mustReturn = true;
//                            break;
//                        }
//                    }
//                    if (!mustReturn)
//                        Array.Resize(ref _handles, count * 2);
//                }

//                if (mustReturn)
//                    return;
//            }

//            var handle = _handles[count];
//            if (handle.IsAllocated)
//            {
//                handle.Target = item;
//            }
//            else
//            {
//                try
//                {
//                }
//                finally
//                {
//                    _handles[count] = GCHandle.Alloc(item, GCHandleType.Weak);
//                }
//            }

//            Count++;
//        }

//        /// <summary>
//        /// Tries to remove an item from this list and returns if it was
//        /// found and removed. Note that the Count and the following items 
//        /// are not automatically updated, as this will only happen in the next
//        /// garbage collection.
//        /// </summary>
//        public bool Remove(T item)
//        {
//            _syncObj.EnterWriteLock();
//            try
//            {
//                int index = _IndexOf(item);
//                if (index != -1)
//                {
//                    _handles[index].Target = null;
//                    if (index == Count - 1)
//                        Count = index;
//                    return true;
//                }
//            }
//            finally
//            {
//                _syncObj.EnterWriteLock();
//            }
//            return false;
//        }

//        public void RemoveAt(int index)
//        {
//            _syncObj.EnterWriteLock();
//            try
//            {
//                _handles[index].Target = null;
//            }
//            finally
//            {
//                _syncObj.EnterWriteLock();
//            }
//        }

//        /// <summary>
//        /// Clears all the items in the list.
//        /// </summary>
//        public void Clear()
//        {
//            _syncObj.EnterWriteLock();
//            try
//            {
//                AssertNotDisposed();
//                int count = Count;
//                for (int i = count - 1; i >= 0; i--)
//                {
//                    var handle = _handles[i];
//                    if (handle.IsAllocated)
//                        handle.Free();
//                }
//                Count = 0;
//            }
//            finally
//            {
//                _syncObj.EnterWriteLock();
//            }
//        }

//        /// <summary>
//        /// Returns true if an item exists in the collection, false otherwise.
//        /// </summary>
//        public bool Contains(T item)
//        {
//            if (item == null) throw new ArgumentNullException("item");
//            _syncObj.EnterReadLock();
//            try
//            {
//                return _IndexOf(item) != -1;
//            }
//            finally
//            {
//                _syncObj.ExitReadLock();
//            }
//        }

//        int _IndexOf(T item)
//        {
//            AssertNotDisposed();
//            int count = Count;
//            for (int i = 0; i < count; i++)
//            {
//                GCHandle handle = _handles[i];
//                if (handle.Target == item)
//                    return i;
//            }
//            return -1;
//        }

//        /// <summary>
//        /// Gets a strong-list with all the non-collected items present
//        /// in this list.
//        /// </summary>
//        public List<T> ToList()
//        {
//            var result = new List<T>();
//            try
//            {
//                AssertNotDisposed();
//                int count = Count;
//                for (int i = 0; i < count; i++)
//                {
//                    object target = _handles[i].Target;
//                    if (target != null)
//                        result.Add((T)target);
//                }
//            }
//            finally
//            {
//                _syncObj.ExitReadLock();
//            }
//            return result;
//        }

//        static int _SortHandles(GCHandle a, GCHandle b)
//        {
//            if (a.Target == null)
//                return b.Target == null ? 0 : 1;
//            return b.Target == null ? -1 : 0;
//        }

//        #region ICollection<T> Members

//        void ICollection<T>.CopyTo(T[] array, int arrayIndex)
//        {
//            ToList().CopyTo(array, arrayIndex);
//        }
//        bool ICollection<T>.IsReadOnly
//        {
//            get { return false; }
//        }

//        #endregion

//        #region IEnumerator<T>

//        /// <summary>
//        /// Gets an enumerator over the non-collected items of this
//        /// list.
//        /// </summary>
//        public IEnumerator<T> GetEnumerator()
//        {
//            return ToList().GetEnumerator();
//        }
//        IEnumerator IEnumerable.GetEnumerator()
//        {
//            return GetEnumerator();
//        }

//        #endregion

//        //#region ISerializable Members
//        ///// <summary>
//        ///// Creates the WeakList from the serialization info.
//        ///// </summary>
//        //protected WeakCollection(SerializationInfo info, StreamingContext context) : this(32) { }

//        ///// <summary>
//        ///// Creates serialization info.
//        ///// </summary>
//        //protected virtual void GetObjectData(SerializationInfo info, StreamingContext context) { }

//        //void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
//        //{
//        //    GetObjectData(info, context);
//        //}
//        //#endregion
//    }
//}

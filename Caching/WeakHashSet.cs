//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.Runtime.InteropServices;
//using Pfz.Threading;

//namespace Pfz.Caching
//{
//    /// <summary>
//    /// A hashset that allows items to be collected.
//    /// </summary>
//    public sealed class WeakHashSet<T>:
//        ReaderWriterSafeDisposable,
//        IEnumerable<T>,
//        IGarbageCollectionAware
//    where
//        T: class
//    {
//        private HashSet<ThreadUnsafeReferenceSlim<T>> _hashset = new HashSet<ThreadUnsafeReferenceSlim<T>>();

//        internal WeakHashSet(bool value)
//        {
//        }

//        /// <summary>
//        /// Creates a new weak-hashset.
//        /// </summary>
//        public WeakHashSet()
//        {
//            GCUtils.RegisterForCollectedNotification(this);
//        }

//        /// <summary>
//        /// Releases all resources used by this hashset.
//        /// </summary>
//        protected override void Dispose(bool disposing)
//        {
//            if (disposing)
//            {
//                GCUtils.UnregisterFromCollectedNotification(this);

//                foreach(var item in _hashset)
//                    item.Dispose();
//            }

//            base.Dispose(disposing);
//        }

//        /// <summary>
//        /// Adds an item.
//        /// </summary>
//        public bool Add(T item)
//        {
//            var reference = new ThreadUnsafeReferenceSlim<T>(item, GCHandleType.Weak);
//            try
//            {
//                using(DisposeLock.WriteLock())
//                {
//                    CheckUndisposed();

//                    if (!_hashset.Add(reference))
//                    {
//                        reference.Dispose();
//                        return false;
//                    }

//                    return true;
//                }
//            }
//            catch
//            {
//                reference.Dispose();
//                throw;
//            }
//        }

//        /// <summary>
//        /// Removes an item.
//        /// </summary>
//        public bool Remove(T item)
//        {
//            using(DisposeLock.WriteLock())
//            {
//                CheckUndisposed();

//                using(var reference = new ThreadUnsafeReferenceSlim<T>(item, GCHandleType.Weak))
//                    return _hashset.Remove(reference);
//            }
//        }

//        /// <summary>
//        /// Enumerates all valid (non-collected) items in this hashset.
//        /// </summary>
//        public IEnumerator<T> GetEnumerator()
//        {
//            return ToList().GetEnumerator();
//        }

//        /// <summary>
//        /// Creates a strong-list with all items still available in this hashset.
//        /// </summary>
//        public List<T> ToList()
//        {
//            List<T> result;

//            using(DisposeLock.ReadLock())
//            {
//                CheckUndisposed();

//                result = new List<T>(_hashset.Count);

//                foreach(var reference in _hashset)
//                {
//                    T item = reference.Value;
//                    if (item != null)
//                        result.Add(item);
//                }
//            }

//            return result;
//        }

//        IEnumerator IEnumerable.GetEnumerator()
//        {
//            return GetEnumerator();
//        }

//        void IGarbageCollectionAware.OnCollected()
//        {
//            using(var upgradeableLock = DisposeLock.UpgradeableLock())
//            {
//                if (WasDisposed)
//                    return;

//                List<ThreadUnsafeReferenceSlim<T>> toRemove = null;
//                HashSet<ThreadUnsafeReferenceSlim<T>> newset;
				
//                try
//                {
//                    newset = new HashSet<ThreadUnsafeReferenceSlim<T>>();
//                    foreach(var reference in _hashset)
//                    {
//                        if (reference.Value != null)
//                            newset.Add(reference);
//                        else
//                        {
//                            if (toRemove == null)
//                                toRemove = new List<ThreadUnsafeReferenceSlim<T>>();

//                            toRemove.Add(reference);
//                        }
//                    }
//                }
//                catch(OutOfMemoryException)
//                {
//                    return;
//                }

//                upgradeableLock.Upgrade();

//                _hashset = newset;

//                if (toRemove != null)
//                {
//                    int count = toRemove.Count;
//                    for(int i=0; i<count; i++)
//                        toRemove[i].Dispose();
//                }
//            }
//        }
//    }
//}

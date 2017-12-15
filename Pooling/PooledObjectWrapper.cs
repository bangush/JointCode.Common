//
// Authors:
//   刘静谊 (Johnny Liu) <jingeelio@163.com>
//
// Copyright (c) 2017 刘静谊 (Johnny Liu)
//
// Licensed under the LGPLv3 license. Please see <http://www.gnu.org/licenses/lgpl-3.0.html> for license text.
//

//using System;
//using System.Diagnostics.CodeAnalysis;
//using System.Threading;

//namespace JointCode.Common.Pooling
//{
//    //The object pool pattern is a software creational design pattern that uses a set of initialized objects kept ready to use – a "pool" – rather 
//    //than allocating and destroying them on demand. A client of the pool will request an object from the pool and perform operations on the returned 
//    //object. When the client has finished, it returns the object to the pool rather than destroying it; this can be done manually or automatically.

//    //Object pools are primarily used for performance: in some circumstances, object pools significantly improve performance. Object pools complicate 
//    //object lifetime, as objects obtained from and returned to a pool are not actually created or destroyed at this time, and thus require care in 
//    //implementation.

//    //Object pools can be implemented in an automated fashion in languages like C++ via smart pointers. In the constructor of the smart pointer - an 
//    //object can be requested from the pool and in the destructor of the smart pointer - the object can be released back to the pool. In garbage collected 
//    //languages, where there are no destructors (which are guaranteed to be called as part of a stack unwind) - object pools MUST be implemented in a manual
//    //fashion, by explicitly requesting an object from the factory and returning the object by calling a dispose method (as in the dispose pattern). Using 
//    //a finalizer to do this is not a good idea as there are usually no guarantees on when (or if ever) the finalizer will be run. Instead - prefer using 
//    //try ... finally to ensure that getting and releasing the object is exception neutral.


//    //So, to consider the object pool to be really functional, the right way is to always use a wrapper (PooledObject<>), as following:

//    //The real pooled objects can be huge in memory, take time to create, and so on. So, everytime a new object is requested, no matter whether it 
//    //is created by the pool or simple returned from the pool, a new wrapper is created.

//    //Such wrapper, when it is collected, return the real object to the pool. When it is Disposed, returns the real object to the pool 
//    //and mark itself as disposed. A new dispose will do nothing and all the wrapped methods should check if the object is disposed or not.

//    //In this case we will have a real Dispose working. The real object returns to the pool, but the disposed object is no more utilisable. 
//    //Trying to use it again will tell you that it was disposed. Also, double disposes will work as expected.

//    /// <summary>
//    /// Create a wrapper for the pooled object.
//    /// </summary>
//    /// <remarks>
//    /// The pooled object is not like normal objects, in that once created, its lifetime is controlled 
//    /// by this wrapper, and should not be disturbed by other parts. And the lifetime of this wrapper, 
//    /// in return, is totally controlled and tracked by <see cref="ObjectPool{T}"/>.
//    /// </remarks>
//    public sealed class PooledObjectWrapper<TPooled> : IDisposable
//        where TPooled : class
//    {
//        /// <summary>
//        /// The <see cref="ObjectPool{T}"/> which holds this wrapper.
//        /// </summary>
//        readonly ObjectPool<TPooled> _objectPool;
//        /// <summary>
//        /// A reference to the pooled object which is managed internally by this instance.
//        /// </summary>
//        TPooled _slot;
//        /// <summary>
//        /// An external reference to the pooled object, that will be cleared after a Dispose, 
//        /// and resurrected by the <see cref="ObjectPool{T}"/> later.
//        /// </summary>
//        TPooled _pooled;

//        /// <summary>
//        /// Initializes a new instance of the <see cref="PooledObjectWrapper{TPooled}"/> class.
//        /// </summary>
//        /// <param name="objectPool">The object pool which holds this wrapper.</param>
//        /// <param name="pooled">The pooled object.</param>
//        internal PooledObjectWrapper(ObjectPool<TPooled> objectPool, TPooled pooled)
//        {
//            _objectPool = objectPool;
//            _slot = pooled;
//            _pooled = pooled;
//        }

//        /// <summary>
//        /// Gets a reference to the pooled object.
//        /// </summary>
//        public TPooled Pooled
//        {
//            get
//            {
//                if (_pooled == null)
//                    throw new InvalidOperationException("The reference to the pooled object is null, this is because this wrapper has been disposed, or it has been destroyed!");
//                return _pooled;
//            }
//        }

//        /// <summary>
//        /// Reset the state of pooled object to allow it to be reused by other parts of the application.
//        /// </summary>
//        /// <returns>
//        /// This method must return <c>true</c> if the state has been reset successfully. 
//        /// If this method returns <c>false</c>, it means that by somehow the pooled object's state can not be restored. 
//        /// In that case, the pooled object will be disposed, discarded by the <see cref="ObjectPool{T}"/>, and therefore
//        /// can not be reused again.
//        /// </returns>
//        internal bool ResetState(Func<TPooled, bool> resetState)
//        {
//            return resetState.Invoke(_slot);
//        }

//        /// <summary>
//        /// Restore the reference to pooled object, so it can be reused again.
//        /// </summary>
//        internal void Resurrect()
//        {
//            _pooled = _slot;
//        }

//        /// <summary>
//        /// Destroy the internal state of this wrapper, and allow it to die (waiting for finalizing).
//        /// After this method, the wrapper won't keep a reference to the pooled object, and therefore 
//        /// can not be reused any more.
//        /// </summary>
//        internal void Destroy()
//        {
//            _pooled = null;
//            var slot = _slot;
//            _slot = null;

//            var disposable = slot as IDisposable;
//            if (disposable != null)
//                disposable.Dispose();

//            // The object is being destroyed, resources have already been released deterministically, there is no need to fire the finalizer
//            GC.SuppressFinalize(this);
//        }

//        void ReturnToPool()
//        {
//            //Disposing the wrapper:
//            //1. Set the _pooled to null to prevent using the Pooled from the user code again 
//            //   after the wrapper is disposed, unless the wrapper is resurrected by the pool.
//            //2. Furthermore, make sure the Dispose is only called once, because we don't want 
//            //   to return the same object to the pool twice.
//            var pooled = _pooled;
//            if (pooled == null)
//                return; //this method called, just return

//            //make sure this method is only called once
//            if (Interlocked.CompareExchange(ref _pooled, null, pooled) == null)
//                return; //already disposed, just return

//            _objectPool.Return(this); //return the wrapper to the pool for reuse again.
//        }

//        /// <summary>
//        /// Return the pooled object to the pool, so that it can be reused again.
//        /// </summary>
//        [SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly")]
//        public void Dispose()
//        {
//            ReturnToPool();
//        }

//        /// <summary>
//        /// Finalizes the instance. Should never be called.
//        /// </summary>
//        [SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly")]
//        ~PooledObjectWrapper()
//        {
//            //The object pool have not destroyed this wrapper yet (the pooled object is still alive, i.e, the Destroy method never get called), 
//            //so we should return this wrapper to the pool and keep it alive too
//            if (_slot != null)
//            {
//                _objectPool.Return(this); //register this wrapper to the pool and thus resurrect it again.
//                GC.ReRegisterForFinalize(this); //tell the GC to call the destructor again next time a GC happen (object resurrection)
//            }
//            //The object pool has destroyed this wrapper already (the Destroy method get called), so let it die
//            //else
//            //{
//            //    _objectPool.Destroy(this); //tell the object pool to untrack this object
//            //}
//        }
//    }
//}

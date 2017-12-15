//
// Authors:
//   刘静谊 (Johnny Liu) <jingeelio@163.com>
//
// Copyright (c) 2017 刘静谊 (Johnny Liu)
//
// Licensed under the LGPLv3 license. Please see <http://www.gnu.org/licenses/lgpl-3.0.html> for license text.
//

//using System;
//using System.Threading;
//using JointCode.Common.Collections;

//namespace JointCode.Common.Pooling
//{
//    /// <summary>
//    /// Diagnostics for <see cref="ObjectPool{T}"/>
//    /// </summary>
//    public class ObjectPoolDiagnostics
//    {
//        #region Public Properties and backing fields

//        /// <summary>
//        /// gets the total count of live instances, both in the pool and in use.
//        /// </summary>
//        public int TotalLiveInstancesCount
//        {
//            get { return _totalInstancesCreated - _totalInstancesDestroyed; }
//        }

//        int _objectResetFailedCount;
//        /// <summary>
//        /// gets the count of object reset failures occured while the pool tried to re-add the object into the pool.
//        /// </summary>
//        public int ObjectResetFailedCount
//        {
//            get { return _objectResetFailedCount; }
//        }

//        int _poolObjectHitCount;
//        /// <summary>
//        /// gets the total count of successful accesses. The pool had a spare object to provide to the user without creating it on demand.
//        /// </summary>
//        public int PoolObjectHitCount
//        {
//            get { return _poolObjectHitCount; }
//        }

//        int _poolObjectMissCount;
//        /// <summary>
//        /// gets the total count of unsuccessful accesses. The pool had to create an object in order to satisfy the user request. If the number is high, consider increasing the object minimum limit.
//        /// </summary>
//        public int PoolObjectMissCount
//        {
//            get { return _poolObjectMissCount; }
//        }

//        int _totalInstancesCreated;
//        /// <summary>
//        /// gets the total number of pooled objected created
//        /// </summary>
//        public int TotalInstancesCreated
//        {
//            get { return _totalInstancesCreated; }
//        }

//        int _totalInstancesDestroyed;
//        /// <summary>
//        /// gets the total number of objects destroyes, both in case of an pool overflow, and state corruption.
//        /// </summary>
//        public int TotalInstancesDestroyed
//        {
//            get { return _totalInstancesDestroyed; }
//        }

//        int _poolOverflowCount;
//        /// <summary>
//        /// gets the number of objects been destroyed because the pool was full at the time of returning the object to the pool.
//        /// </summary>
//        public int PoolOverflowCount
//        {
//            get { return _poolOverflowCount; }
//        }

//        int _returnedToPoolCount;
//        /// <summary>
//        /// gets the total count of objects that been successfully returned to the pool
//        /// </summary>
//        public int ReturnedToPoolCount
//        {
//            get { return _returnedToPoolCount; }
//        }
//        #endregion

//        #region Internal Methods for incrementing the counters
//        internal void IncrementObjectsCreatedCount()
//        {
//            Interlocked.Increment(ref _totalInstancesCreated);
//        }

//        internal void IncrementObjectsDestroyedCount()
//        {
//            Interlocked.Increment(ref _totalInstancesDestroyed);
//        }

//        internal void IncrementPoolObjectHitCount()
//        {
//            Interlocked.Increment(ref _poolObjectHitCount);
//        }

//        internal void IncrementPoolObjectMissCount()
//        {
//            Interlocked.Increment(ref _poolObjectMissCount);
//        }

//        internal void IncrementPoolOverflowCount()
//        {
//            Interlocked.Increment(ref _poolOverflowCount);
//        }

//        internal void IncrementResetStateFailedCount()
//        {
//            Interlocked.Increment(ref _objectResetFailedCount);
//        }

//        internal void IncrementReturnedToPoolCount()
//        {
//            Interlocked.Increment(ref _returnedToPoolCount);
//        }
//        #endregion
//    }

//    //Adapted from http://www.codeproject.com/Articles/535735/Implementing-a-Generic-Object-Pool-in-NET:
//    /// <summary>
//    /// A generic object pool implementation
//    /// </summary>
//    /// <remarks>
//    /// A generic object pool with the following functions:
//    /// !. Never block even if the <see cref="MaxCapacity"/> is exceeded.
//    /// !. Expand the capacity (<see cref="CurrentCapacity"/>) on the fly if needed, and shrink to the 
//    ///    <see cref="MaxCapacity"/> if it is exceeded when the pooled objects return, or the <see cref="MinCapacity"/> 
//    ///    or <see cref="MaxCapacity"/> is adjusted.
//    /// !. Support for more than one pool at the time 
//    /// !. Managing all kinds of objects 
//    /// !. Support for custom object creation 
//    /// !. Bound the number of resources to a limit (through <see cref="MaxCapacity"/> and <see cref="MinCapacity"/>)
//    /// !. Support for pre-loading items to the pool (through <see cref="MinCapacity"/>)
//    /// !. Support for concurrency and multithreading scenarios 
//    /// !. Handle pooled objects even when the programmer forgot to explicitly return them to the pool 
//    /// !. Support for resetting the object's state before returning to pool 
//    /// !. Handle graceful release of the resource when it is no longer needed or the object state is corrupted 
//    /// !. Support for objects that are 3rd party components, and we cannot modify their code to suit our requirements 
//    /// !. Diagnostics – Nice to have, but not a must
//    /// </remarks>
//    /// <typeparam name="T">
//    /// The type of the pooled object.
//    /// </typeparam>
//    public class ObjectPool<T> : CriticalDisposable
//        where T : class
//    {
//        #region Consts

//        const int DefaultMinCapacity = 5;
//        const int DefaultMaxCapacity = 25;

//        #endregion

//        #region Private Members

//        // indication flag that states whether a capacity adjusting operation is in progress.
//        // The type is int, altought it looks like that it should be bool - this was done for Interlocked CAS operation (CompareExchange)
//        int _adjustingCapacityCasFlag = 0; // 0 state false
//        // internal container for pooled objects
//        readonly ConcurrentDeque<PooledObjectWrapper<T>> _wrappers;
//        // a delegate to adjust the pool capacity
//        readonly WaitCallback _adjustCapacity;

//        /// <summary>
//        /// Reset the pooled object's state to allow it to be reused by other parts of the application.
//        /// </summary>
//        /// <returns>
//        /// If provided, this method must return <c>true</c> if the state has been reset successfully. 
//        /// If this method returns <c>false</c>, it means that by somehow the pooled object's state can not be restored. 
//        /// In that case, the pooled object will be disposed, discarded by the <see cref="ObjectPool{T}"/>, and therefore
//        /// can not be reused again.
//        /// </returns>
//        readonly Func<T, bool> _resetState;
//        /// <summary>
//        /// Gets the factory method used to create new pooled objects. 
//        /// </summary>
//        readonly Func<T> _factoryMethod;
//        //readonly MyAction<T> _releaseResource;
//        #endregion

//        #region Public Properties

//        /// <summary>
//        /// Gets the Diagnostics class for the current Object Pool.
//        /// </summary>
//        readonly ObjectPoolDiagnostics _diagnostics;
//        public ObjectPoolDiagnostics Diagnostics
//        {
//            get { return _diagnostics; }
//        }

//        int _currentCapacity;
//        /// <summary>
//        /// Gets the count of objects currently maintained by the pool.
//        /// </summary>
//        public int CurrentCapacity
//        {
//            get { return _currentCapacity; }
//        }

//        int _maxCapacity;
//        /// <summary>
//        /// Gets or sets the maximum number of pooled objects managed by this pool. 
//        /// Do note that this is not a hard limit, i.e, whenever the pool is being 
//        /// asked for an object, even if this limit is reached, a new object is still
//        /// being created and returned to the user. 
//        /// </summary>
//        public int MaxCapacity
//        {
//            get { return _maxCapacity; }
//            set
//            {
//                // validating pool limits, exception is thrown if parameter is invalid
//                ValidateParameters(_minCapacity, value);
//                _maxCapacity = value;
//                AdjustCapacityIfNeccessary();
//            }
//        }

//        int _minCapacity;
//        /// <summary>
//        /// Gets or sets the minimum number of pooled objects in the pool.
//        /// </summary>
//        public int MinCapacity
//        {
//            get { return _minCapacity; }
//            set
//            {
//                // validating pool limits, exception is thrown if parameter is invalid
//                ValidateParameters(value, _maxCapacity);
//                _minCapacity = value;
//                AdjustCapacityIfNeccessary();
//            }
//        }

//        #endregion

//        #region Ctor and Initialization code

//        /// <summary>
//        /// Initializes a new pool with default settings.
//        /// </summary>
//        public ObjectPool()
//            : this(DefaultMinCapacity, DefaultMaxCapacity, null, null)
//        {
//        }

//        /// <summary>
//        /// Initializes a new pool with specified minimum pool size and maximum pool size
//        /// </summary>
//        /// <param name="minCapacity">The minimum pool size limit.</param>
//        /// <param name="maxCapacity">The maximum pool size limit</param>
//        public ObjectPool(int minCapacity, int maxCapacity)
//            : this(minCapacity, maxCapacity, null, null)
//        {
//        }

//        /// <summary>
//        /// Initializes a new pool with specified factory method.
//        /// </summary>
//        /// <param name="factoryMethod">The factory method that will be used to create new objects.</param>
//        /// <param name="resetState">The delegate that will reset the internal state of the pooled object.</param>
//        public ObjectPool(Func<T> factoryMethod, Func<T, bool> resetState)
//            : this(DefaultMinCapacity, DefaultMaxCapacity, factoryMethod, resetState)
//        {
//        }

//        /// <summary>
//        /// Initializes a new pool with specified factory method and minimum and maximum size.
//        /// </summary>
//        /// <param name="minCapacity">The minimum pool size limit.</param>
//        /// <param name="maxCapacity">The maximum pool size limit</param>
//        /// <param name="factoryMethod">The factory method that will be used to create new objects.</param>
//        /// <param name="resetState">The delegate that will reset the internal state of the pooled object.</param>
//        public ObjectPool(int minCapacity, int maxCapacity, Func<T> factoryMethod, Func<T, bool> resetState)
//        {
//            // validating pool limits, exception is thrown if invalid
//            ValidateParameters(minCapacity, maxCapacity);

//            _factoryMethod = factoryMethod;
//            _resetState = resetState;
//            _minCapacity = minCapacity;
//            _maxCapacity = maxCapacity;

//            // Initializing the internal pool data structure
//            _wrappers = new ConcurrentDeque<PooledObjectWrapper<T>>();
//            // Creating a new instance for the Diagnostics class
//            _diagnostics = new ObjectPoolDiagnostics();
//            // Cache a callback delegate in a private field to improve performance, 
//            // because we don't want to create a new delegate each time a ThreadPool based thread is creating.
//            _adjustCapacity = (o) => AdjustCapacity();

//            AdjustCapacityIfNeccessary();
//        }

//        #endregion

//        #region Pool Operations

//        /// <summary>
//        /// Grab a wrapper from the pool.
//        /// </summary>
//        /// <returns>A a wrapper for the pooled object</returns>
//        public PooledObjectWrapper<T> Take()
//        {
//            PooledObjectWrapper<T> wrapper;
//            if (_wrappers.TryPopRight(out wrapper))
//            {
//                _diagnostics.IncrementPoolObjectHitCount(); // diagnostics update
//                return wrapper;
//            }
//            else
//            {
//                // Diagnostics update
//                _diagnostics.IncrementPoolObjectMissCount();
//                return CreatePooledObjectWrapper();
//            }
//        }

//        #endregion

//        #region Private Methods

//        static void ValidateParameters(int minCapacity, int maxCapacity)
//        {
//            if (minCapacity < 0)
//                throw new ArgumentException("Minimum capacity must be greater or equals to zero.");

//            if (maxCapacity < 1)
//                throw new ArgumentException("Maximum capacity must be greater than zero.");

//            if (minCapacity > maxCapacity)
//                throw new ArgumentException("Maximum capacity must be greater than the minimum capacity.");
//        }

//        /// <summary>
//        /// Make sure there is always at least <see cref="MinCapacity"/> objects in the pool.
//        /// </summary>
//        void AdjustCapacityIfNeccessary()
//        {
//            if (_currentCapacity >= _minCapacity && _currentCapacity <= _maxCapacity)
//                return;
//            // if there is an adjusting operation in progress, skip and return. 
//            if (Interlocked.CompareExchange(ref _adjustingCapacityCasFlag, 1, 0) != 0)
//                return;
//            // possibly create new pooled objects to fill the pool.
//            // because the creation of pooled objects might be costly (objects might be huge), 
//            // we use a background thread to do it, and make sure that expand is executed only once.
//            ThreadPool.QueueUserWorkItem(_adjustCapacity);
//            // finished adjusting, allowing additional callers to enter when needed
//            _adjustingCapacityCasFlag = 0;
//        }

//        void AdjustCapacity()
//        {
//            if (_currentCapacity < _minCapacity)
//            {
//                while (_currentCapacity < _minCapacity)
//                {
//                    // always push new items to the left of pool
//                    _wrappers.PushLeft(CreatePooledObjectWrapper());
//                }
//            }
//            else if (_currentCapacity > _maxCapacity)
//            {
//                while (_currentCapacity > _maxCapacity)
//                {
//                    PooledObjectWrapper<T> wrapper;
//                    // always discard items from the left of pool (the less used items)
//                    if (_wrappers.TryPopLeft(out wrapper))
//                    {
//                        // Diagnostics update
//                        Diagnostics.IncrementPoolOverflowCount();
//                        Destroy(wrapper);
//                    }
//                }
//            }
//        }

//        /// <summary>
//        /// Creates a new pooled object and a wrapper for it.
//        /// </summary>
//        /// <returns></returns>
//        PooledObjectWrapper<T> CreatePooledObjectWrapper()
//        {
//            T pooledObj;
//            if (_factoryMethod != null)
//            {
//                pooledObj = _factoryMethod();
//            }
//            else
//            {
//                // Throws an exception if the type doesn't have default ctor - on purpose! 
//                // I've could've add a generic constraint with new (), but I didn't want to limit the user and force a parameterless ctor
//                pooledObj = (T)Activator.CreateInstance(typeof(T));
//            }

//            var wrapper = new PooledObjectWrapper<T>(this, pooledObj);

//            // Diagnostics update
//            _diagnostics.IncrementObjectsCreatedCount();
//            _currentCapacity += 1;

//            return wrapper;
//        }

//        /// <summary>
//        /// Destroys the internal state of specified wrapper.
//        /// </summary>
//        /// <param name="wrapper">The wrapper.</param>
//        void Destroy(PooledObjectWrapper<T> wrapper)
//        {
//            _currentCapacity -= 1;
//            wrapper.Destroy();
//            _diagnostics.IncrementObjectsDestroyedCount();
//        }

//        #endregion

//        #region Called by PooledObjectWrapper

//        internal void Return(PooledObjectWrapper<T> wrapper)
//        {
//            if (_currentCapacity <= _maxCapacity)
//            {
//                if (_resetState != null && !wrapper.ResetState(_resetState))
//                {
//                    //if we failed to restore the state for pooled object: 
//                    //destroy the wrapper and the pooled object it managed, and discard it.
//                    Destroy(wrapper);
//                    _diagnostics.IncrementResetStateFailedCount(); // diagnostics update
//                    AdjustCapacityIfNeccessary();
//                }
//                else
//                {
//                    //resurrect the wrapper for later reuse.
//                    wrapper.Resurrect();
//                    // always return the object to the right of pool, and take object from the right of pool, 
//                    // this will make sure we always reuse the same objects as much as possible.
//                    _wrappers.PushRight(wrapper);
//                    _diagnostics.IncrementReturnedToPoolCount(); // diagnostics update
//                }
//            }
//            else
//            {
//                //The pool's upper limit has exceeded, there is no need to add this object back into the pool and we can destroy it.
//                Destroy(wrapper);
//                _diagnostics.IncrementPoolOverflowCount(); // diagnostics update
//                AdjustCapacityIfNeccessary();
//            }
//        }

//        #endregion

//        #region Dispose

//        protected override void Dispose(bool disposing)
//        {
//            // The pool is going down, releasing the resources for all objects in pool
//            foreach (var wrapper in _wrappers)
//                wrapper.Destroy();
//        }

//        #endregion
//    }
//}
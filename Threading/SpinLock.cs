using System;
using System.Threading;

namespace JointCode.Common.Threading
{
    /// <summary>
    /// 	A spinlock is a lock where the thread simply waits in a loop ("spins") repeatedly checking until the lock becomes available.
    /// </summary>
    public class Spinlock
    {
        /// <summary>
        /// 	The locks owner.
        /// </summary>
        private Thread _owner;

        /// <summary>
        /// 	Lock recursion.
        /// </summary>
        private int _recursion;

        /// <summary>
        /// 	Is <c>true</c> if this lock is currently held.
        /// </summary>
        public bool IsLocked
        {
            get { return _recursion > 0; }
        }

        /// <summary>
        /// 	Is <c>true</c> if the current owner it the current thread.
        /// </summary>
        public bool IsOwned
        {
            get { return _owner == Thread.CurrentThread; }
        }

        /// <summary>
        /// 	Enters the lock. The calling thread will spin wait until it gains ownership of the lock.
        /// </summary>
        public void Enter()
        {
            while (!TryEnter())
            {
            }
        }

        /// <summary>
        /// 	Tries to enter the lock.
        /// </summary>
        /// <returns><c>true</c> if the lock was successfully taken; else <c>false</c>.</returns>
        public bool TryEnter()
        {
            // get the current thead
            var caller = Thread.CurrentThread;
            // early out: return if the current thread already has ownership.
            if (_owner == caller)
            {
                Interlocked.Increment(ref _recursion);
                return true;
            }
            // try to take the lock, if the current owner is null.
            var success = Interlocked.CompareExchange(ref _owner, caller, null) == null;
            if (success)
                Interlocked.Increment(ref _recursion);
            return success;
        }

        /// <summary>
        /// 	Tries to enter the lock.
        /// 	Fails after the specified time has elapsed without aquiring the lock.
        /// </summary>
        /// <param name = "timeout">The timeout.</param>
        /// <returns><c>true</c> if the lock was successfully taken; else <c>false</c>.</returns>
        public bool TryEnter(TimeSpan timeout)
        {
            var start = DateTime.Now;
            while (!TryEnter())
            {
                if (DateTime.Now - start > timeout)
                    return false;
            }
            return true;
        }

        /// <summary>
        /// 	Exits the lock. This allows other threads to take ownership of the lock.
        /// </summary>
        /// <exception cref="InvalidOperationException"><c>InvalidOperationException</c>.</exception>
        public void Exit()
        {
            // get the current thread.
            var caller = Thread.CurrentThread;
            if (caller != _owner) throw new InvalidOperationException("Validation.AccessViolation");
            Interlocked.Decrement(ref _recursion);
            if (_recursion == 0)
                _owner = null;
        }
    }
}
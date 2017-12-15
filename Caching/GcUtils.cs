using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

#if SILVERLIGHT
using System.Windows;
#endif

namespace JointCode.Common.Caching
{
    /// Caution: GC.KeepAlive keeps the object alive until that line of code,
    /// while GCUtils.KeepAlive keeps the object alive until the next 
    /// generation.
    /// 
    /// <summary>
    /// Some methods and events to interact with garbage collection. You can 
    /// register to know when a collection has just happened. This is useful 
    /// if you don't use WeakReferences, but know how to free memory if needed. 
    /// For example, you can do a TrimExcess to your lists to free some memory.
    /// </summary>
    public static partial class GcUtils
    {
        static bool _processOrDomainExiting;
        static long _processMemory;
        static readonly object _syncObj = new object();
        static List<IGcObserver> _gcObservers = new List<IGcObserver>();

        [SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
        static GcUtils()
        {
#if SILVERLIGHT
            Application.Current.Exit += _ProcessExit;
#else
            AppDomain current = AppDomain.CurrentDomain;
            current.DomainUnload += _ProcessExit;
            current.ProcessExit += _ProcessExit;
#endif
            var runner = new Runner();
            runner.DoNothing();

            // Using another thread to perform action when GC happens -- noted by jingyi.liu
            Thread collectorThread = new Thread(_ExecuteCollected);

#if !SILVERLIGHT
            collectorThread.Priority = ThreadPriority.AboveNormal;
#endif

            collectorThread.Start();
        }

        /// <summary>
        /// Gets or sets a value that indicates how much memory the process
        /// can use without freeing it's caches.
        /// Note that such value does not affect how often GC occurs and is
        /// not the size of the cache, it only says: If my process is not using
        /// more than X memory, caches don't need to be erased.
        /// The default value is 0 mb.
        /// </summary>
        public static long ProcessMemory
        {
            get { return _processMemory; }
            set { _processMemory = value; }
        }

        static void _ProcessExit(object sender, EventArgs e)
        {
            // -- noted by jingyi.liu
            // Indicates that we are exiting the application or appdomain. 
            // Otherwise, the Runner instance will be kept alive, which will prevent the application or appdomain to end.
            _processOrDomainExiting = true;
            lock (_syncObj)
                Monitor.Pulse(_syncObj);
        }

        static void _ExecuteCollected()
        {
            var thread = Thread.CurrentThread;
            while (true)
            {
                // we are background while waiting.
                thread.IsBackground = true;

                lock (_syncObj)
                {
                    if (_processOrDomainExiting)
                        return;
                    // -- noted by jingyi.liu
                    // Waiting for GC notification from the Runner instance to execute _ExecuteCollectedNow method once.
                    Monitor.Wait(_syncObj);
                }

                if (_processOrDomainExiting)
                    return;

                // but we are not background while running.
                thread.IsBackground = false;
                _ExecuteCollectedNow();
            }
        }

        static void _ExecuteCollectedNow()
        {
            // -- noted by jingyi.liu
            // Get an immutable version of the GarbageCollectionNotifiers because we may run in multi-thread enviroment.
            var gcNotifiers = _gcObservers.ToArray();
            int count = gcNotifiers.Length;
            for (int i = 0; i < count; i++)
                gcNotifiers[i].OnGarbageCollected();
        }

        /// <summary>
        /// Registers the given object to the GcCollected notification.
        /// </summary>
        public static void RegisterGcObserver(IGcObserver gcObserver)
        {
            // -- noted by jingyi.liu
            // The _gcObservers itself and all of its elements needs to be weak too, in case the subscriber forget to call 
            // the UnregisterGarbageCollectionNotifier, there would be memory leaks.
            if (gcObserver == null) return;
            _gcObservers.Add(gcObserver);
        }

        /// <summary>
        /// Unregisters the given object from the GcCollected notification.
        /// </summary>
        public static void UnregisterGcObserver(IGcObserver gcObserver)
        {
            // -- noted by jingyi.liu
            // The _gcObservers itself and all of its elements needs to be weak too, in case the subscriber forget to call 
            // the UnregisterGarbageCollectionNotifier, there would be memory leaks.
            if (gcObserver == null) return;
            _gcObservers.Remove(gcObserver);
        }
    }

    partial class GcUtils
    {
        sealed class Runner
        {
            [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
            public void DoNothing() { }

            ~Runner()
            {
                // If we don't test, we will keep re-registering forever
                // when the application is finishing.
                if (_processOrDomainExiting)
                    return;

                GC.ReRegisterForFinalize(this);

                if (GC.GetTotalMemory(false) <= _processMemory)
                    return;

                // does not need to be thread-safe, as this is the
                // destructor thread, but if the lock is not got
                // we should wait the next collection, as we
                // can't block while all threads may be suspended.
                if (Monitor.TryEnter(_syncObj, 0))
                {
                    try
                    {
                        Monitor.Pulse(_syncObj);
                    }
                    finally
                    {
                        Monitor.Exit(_syncObj);
                    }
                }
            }
        }
    }
}

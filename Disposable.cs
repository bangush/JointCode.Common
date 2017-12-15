//
// Authors:
//   刘静谊 (Johnny Liu) <jingeelio@163.com>
//
// Copyright (c) 2017 刘静谊 (Johnny Liu)
//
// Licensed under the LGPLv3 license. Please see <http://www.gnu.org/licenses/lgpl-3.0.html> for license text.
//

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.ConstrainedExecution;
using System.Threading;

namespace JointCode.Common
{
    // 关于 CLR 的 Finalizer：
    // The finalizer is only called during the Finalization phase, which will happen during GC.
    // The Finalize method is used to perform cleanup operations on unmanaged resources held by the current object before the object is destroyed. 

    //对于非托管资源，建议使用SafeHandle系列管理其句柄，其基类CriticalFinalizerObject有如下CLR级别支持的额外特性：
    //1、 从CriticalFinalizerObject继承的类型首次被引用时，会JIT其析构函数，确保其在析构时不会因内存不足而失败。
    //2、 在析构时，会优先析构其他不是从CriticalFinalizerObject继承的对象，使得在普通类型的析构函数中可以使用CriticalFinalizerObject类型的对象。
    //3、 当整个AppDomain被强行卸载时，CriticalFinalizerObject对象的析构函数仍然会被调用。

    //SafeHandle 比 CriticalFinalizerObject 具有更多功能，且专为与非托管代码的交互而进行了优化。此外：
    //1、 SafeHandle对象可以在P/Invoke时替代IntPtr作为参数和返回类型，确保异常安全。
    //2、 P/Invoke时会正确管理内部的引用计数，确保多线程引用的情况下不会被提前意外释放。
    //3、 CriticalHandle是不带引用计数的SafeHandle。
    //4、 SafeHandle和CriticalHandle及其子类都是抽象类，在具体场景需要通过继承的方式使用。

    // CriticalFinalizerObject 的作用：
    //Firstly, let's read about CriticalFinalizerObject in MSDN, we can read, that:
    //In classes derived from the CriticalFinalizerObject class, the common language runtime (CLR) guarantees that all critical finalization code will 
    //be given the opportunity to execute, provided the finalizer follows the rules for a Constrained Execution Region (CER), even in situations where 
    //the CLR forcibly unloads an application domain or aborts a thread.
    //The main word here is UNLOAD.

    //Secondly, let's read MSDN again, this time about Exceptions in managed threads:
    //If these exceptions are unhandled in the main thread, or in threads that entered the runtime from unmanaged code, they proceed normally, resulting 
    //in termination of the application.
    //The main word is TERMINATION.

    //So, when there is an unhandled exception in main thread, the application terminates, but the CriticalFinalizerObject helps only on unloading of Domain.


    /// <summary>
    /// Provides a base class for implementing the disposable pattern.
    /// Note that this class does not provide a finalizer.
    /// </summary>
    public abstract class Disposable : IDisposable
    {
        bool _disposed;

        /// <summary>
        /// Disposes the instance.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly")]
        public void Dispose()
        {
            if (_disposed) return;
            DoDispose();
            _disposed = true;
        }

        public bool Disposed
        {
            get { return _disposed; }
        }

        /// <summary>
        /// Releases resources.
        /// </summary>
        protected virtual void DoDispose()
        {
        }

        protected void AssertNotDisposed()
        {
            if (_disposed)
                throw new InvalidOperationException(string.Format("The current instance ({0}) has been disposed!", GetType().FullName));
        }
    }

    /// <summary>
    /// Provides a base class for implementing the disposable pattern.
    /// Note that this class derives from <see cref="CriticalFinalizerObject"/>.
    /// </summary>
    public abstract class CriticalDisposable : CriticalFinalizerObject, IDisposable
    {
        bool _disposed;

        /// <summary>
        /// Finalizes the instance. Should never be called.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly")]
        ~CriticalDisposable()
        {
            Dispose(false);
        }

        /// <summary>
        /// Disposes the instance.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly")]
        public void Dispose()
        {
            if (_disposed) return;
            Dispose(true);
            _disposed = true;
            GC.SuppressFinalize(this);
        }

        public bool Disposed
        {
            get { return _disposed; }
        }

        /// <summary>
        /// Releases resources.
        /// </summary>
        /// <param name="disposing">
        /// <c>true</c> to release both managed and unmanaged resources; 
        /// <c>false</c> to release only unmanaged resources.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            //if (disposing)
            //    DisposeManagedResources();
            //DisposeUnmanagedResources();
        }

        protected void AssertNotDisposed()
        {
            if (_disposed)
                throw new InvalidOperationException(string.Format("The current instance ({0}) has been disposed!", GetType().FullName));
        }
    }

    /// <summary>
    /// Provides a base class for implementing the disposable pattern.
    /// Note that this class assures that the <see cref="Dispose"/> method will be called only once, 
    /// and it derives from <see cref="CriticalFinalizerObject"/>.
    /// </summary>
    public abstract class ThreadSafeCriticalDisposable : CriticalFinalizerObject, IDisposable
    {
        const int DisposedFlag = 1;
        const int UndisposedFlag = 0;
        int _disposed = UndisposedFlag;

        /// <summary>
        /// Finalizes the instance. Should never be called.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly")]
        ~ThreadSafeCriticalDisposable()
        {
            Dispose(false);
        }

        /// <summary>
        /// Disposes the instance.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly")]
        public void Dispose()
        {
            if (Interlocked.CompareExchange(ref _disposed, DisposedFlag, UndisposedFlag) != UndisposedFlag) return;
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public bool Disposed
        {
            get { return _disposed == DisposedFlag; }
        }

        /// <summary>
        /// Releases resources.
        /// </summary>
        /// <param name="disposing">
        /// <c>true</c> to release both managed and unmanaged resources; 
        /// <c>false</c> to release only unmanaged resources.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            //if (disposing)
            //    DisposeManagedResources();
            //DisposeUnmanagedResources();
        }

        protected void AssertNotDisposed()
        {
            if (_disposed == DisposedFlag)
                throw new InvalidOperationException(string.Format("The current instance ({0}) has been disposed!", GetType().FullName));
        }
    }
}
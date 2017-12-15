//
// Authors:
//   刘静谊 (Johnny Liu) <jingeelio@163.com>
//
// Copyright (c) 2017 刘静谊 (Johnny Liu)
//
// Licensed under the LGPLv3 license. Please see <http://www.gnu.org/licenses/lgpl-3.0.html> for license text.
//

using System;

namespace JointCode.Common.ExceptionHandling
{
    #region Todo
    /// Todo:
    /// 应当根据策略来配置应用程序域出现未处理异常时的响应机制。例如，出现未处理异常时，可以从以下几个方面来考虑：
    /// 1. 结束当前应用程序（优雅方式或粗鲁方式）
    /// 2. 不结束当前应用程序
    /// 2.1 是否结束出现错误的应用程序域
    /// 2.2 默认应用程序域出现未处理异常时的自定义操作（例如记录日志或发送消息等）
    /// 2.2 非默认应用程序域出现未处理异常时的自定义操作（例如记录日志或发送消息等）
    /// 2.3 非默认应用程序域出现未处理异常时，如何将其异常传播到默认应用程序域（因为自定义异常可能是不可序列化的）

    /// According to Microsoft, AppDomains are the "lightweight processes", but there is one thing the AppDomains do not provide you with, 
    /// while processes do: fault tolerance. The operating system doesn't crash if one of its processes crashes. But as of .Net 2.0, an 
    /// unhandled exception in any AppDomain brings the whole application down.
    /// One way is to revert to .Net 1.0/1.1 policy for unhandled exceptions, AND (this is very important) to emulate .Net 2.0 policy for 
    /// the main Appdomain, and to unload other domains, should they encounter an unhandled exception.
    /// 仅当未处理异常 (unhandled exceptions) 不是在主线程抛出时，使用 .Net 1.0/1.1 的异常处理策略才能避免应用程序崩溃。
    /// 也就是说，如果从主 AppDomain 调用其他 AppDomain 的方法，则调用代码无论如何都需要使用 try catch 进行保护。
    /// 在这种情况下，由其他 AppDomain 自身引起的异常必然是在其他线程上引起的，因此还是可以考虑使用上述策略以避免应用程序崩溃。 
    #endregion

    //class UnhandledExceptionHandler
    //{
    //    internal static void OnUnhandledException(AppDomain domain)
    //    {
    //        // 此处必须挂接 UnhandledException 事件，以便实现 ShuttleDomain 以及非托管代码的生存期管理
    //        // 此方法内部需要调用非托管代码的 UnregisterDomain 函数，否则将会造成非托管代码内存泄漏
    //        if (domain.IsDefaultAppDomain())
    //            domain.UnhandledException += DefaultAppDomain_OnUnhandledException;
    //        else
    //            domain.UnhandledException += ChildAppDomain_OnUnhandledException;
    //    }

    //    // Notice the UE, kill offending AppDomain, CLR stays alive.
    //    // The only fun part here is in distinguishing between the exceptions that came from the main AppDomain and all other domains.
    //    // (ExceptionMarker adds a special marker to the Exception's property bag - in the subdomain, and then reads the marker in main domain; if there is no marker attached to exception, then it originates from the main AppDomain, and the process is doomed.  
    //    static void DefaultAppDomain_OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
    //    {
    //        var failingDomain = sender as AppDomain;
    //        if (!failingDomain.IsDefaultAppDomain())
    //            AppDomain.Unload(failingDomain);

    //        //// The following code runs in main AppDomain
    //        //Exception exception = e.ExceptionObject as Exception;

    //        //if (exception == null)
    //        //{
    //        //    Environment.FailFast("Very descriptive message");
    //        //}

    //        //Console.WriteLine("Got an exception in main application domain.");

    //        //try
    //        //{
    //        //    Marker exceptionMarker = ExceptionMarker.GetExceptionMarker(exception);

    //        //    Console.WriteLine("Retrieved marker: " + exceptionMarker.Value);
    //        //    Console.WriteLine("Offending AppDomain: " + _subDomain.Id);

    //        //    if (exceptionMarker.Value.Equals(_subDomain.Id.ToString(), StringComparison.Ordinal))
    //        //    {
    //        //        new Thread(delegate()
    //        //        {
    //        //            try
    //        //            {
    //        //                Console.WriteLine("Unloading the offending application domain...");

    //        //                AppDomain.Unload(_subDomain);
    //        //                _subDomain = null;

    //        //                Console.WriteLine("Unloaded the offending application domain...");
    //        //            }
    //        //            catch (Exception ex)
    //        //            {
    //        //                Console.WriteLine(ex.ToString());
    //        //            }
    //        //        }).Start();
    //        //    }
    //        //}
    //        //catch (InvalidOperationException)
    //        //{
    //        //    // exception is not marked - originates from the main application domain. Demonstration code - a more specific exception type is required.
    //        //    Environment.FailFast("Terminating to an unhandled exception in the main Application Domain.");
    //        //}
    //    }

    //    static void ChildAppDomain_OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
    //    {
    //        var failingDomain = sender as AppDomain;
    //        if (!failingDomain.IsDefaultAppDomain())
    //            Console.WriteLine(failingDomain.FriendlyName);

    //        //// The following code runs in child appdomain
    //        //Exception exception = e.ExceptionObject as Exception;

    //        //if (exception == null)
    //        //{
    //        //    Environment.FailFast("Very descriptive message");
    //        //}

    //        //ExceptionMarker.MarkException(exception, new Marker(AppDomain.CurrentDomain.Id.ToString()));
    //    }
    //}
}
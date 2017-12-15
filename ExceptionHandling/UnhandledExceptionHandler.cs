//
// Authors:
//   ������ (Johnny Liu) <jingeelio@163.com>
//
// Copyright (c) 2017 ������ (Johnny Liu)
//
// Licensed under the LGPLv3 license. Please see <http://www.gnu.org/licenses/lgpl-3.0.html> for license text.
//

using System;

namespace JointCode.Common.ExceptionHandling
{
    #region Todo
    /// Todo:
    /// Ӧ�����ݲ���������Ӧ�ó��������δ�����쳣ʱ����Ӧ���ơ����磬����δ�����쳣ʱ�����Դ����¼������������ǣ�
    /// 1. ������ǰӦ�ó������ŷ�ʽ���³��ʽ��
    /// 2. ��������ǰӦ�ó���
    /// 2.1 �Ƿ�������ִ����Ӧ�ó�����
    /// 2.2 Ĭ��Ӧ�ó��������δ�����쳣ʱ���Զ�������������¼��־������Ϣ�ȣ�
    /// 2.2 ��Ĭ��Ӧ�ó��������δ�����쳣ʱ���Զ�������������¼��־������Ϣ�ȣ�
    /// 2.3 ��Ĭ��Ӧ�ó��������δ�����쳣ʱ����ν����쳣������Ĭ��Ӧ�ó�������Ϊ�Զ����쳣�����ǲ������л��ģ�

    /// According to Microsoft, AppDomains are the "lightweight processes", but there is one thing the AppDomains do not provide you with, 
    /// while processes do: fault tolerance. The operating system doesn't crash if one of its processes crashes. But as of .Net 2.0, an 
    /// unhandled exception in any AppDomain brings the whole application down.
    /// One way is to revert to .Net 1.0/1.1 policy for unhandled exceptions, AND (this is very important) to emulate .Net 2.0 policy for 
    /// the main Appdomain, and to unload other domains, should they encounter an unhandled exception.
    /// ����δ�����쳣 (unhandled exceptions) ���������߳��׳�ʱ��ʹ�� .Net 1.0/1.1 ���쳣������Բ��ܱ���Ӧ�ó��������
    /// Ҳ����˵��������� AppDomain �������� AppDomain �ķ���������ô���������ζ���Ҫʹ�� try catch ���б�����
    /// ����������£������� AppDomain ����������쳣��Ȼ���������߳�������ģ���˻��ǿ��Կ���ʹ�����������Ա���Ӧ�ó�������� 
    #endregion

    //class UnhandledExceptionHandler
    //{
    //    internal static void OnUnhandledException(AppDomain domain)
    //    {
    //        // �˴�����ҽ� UnhandledException �¼����Ա�ʵ�� ShuttleDomain �Լ����йܴ���������ڹ���
    //        // �˷����ڲ���Ҫ���÷��йܴ���� UnregisterDomain ���������򽫻���ɷ��йܴ����ڴ�й©
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
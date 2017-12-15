//
// Authors:
//   刘静谊 (Johnny Liu) <jingeelio@163.com>
//
// Copyright (c) 2017 刘静谊 (Johnny Liu)
//
// Licensed under the LGPLv3 license. Please see <http://www.gnu.org/licenses/lgpl-3.0.html> for license text.
//

using System;
using System.Diagnostics;
using System.Security;
using JointCode.Common.Logging;

namespace JointCode.Common.Helpers
{
    public static class DebugHelper
    {
        static readonly ILogger _logger =
            new DefaultLogger("", LogLevel.Trace, 
                DefaultFileTarget.GetInstance(new FileLogSetting(null, "debug.log")), 
                new DefaultLogFormatter("{timestamp} - {message}{newline}"));

        [Conditional("DEBUG")]
        public static void WriteToConsole(string message)
        {
            Console.WriteLine(message);
        }

        [Conditional("DEBUG")]
        [SecuritySafeCritical]
        public static void WriteToConsole(string message, params object[] args)
        {
            Console.WriteLine(message, args);
        }

        [Conditional("DEBUG")]
        [SecuritySafeCritical]
        public static void WriteToFile(string message)
        {
            _logger.Trace(message);
        }

        [Conditional("DEBUG")]
        [SecuritySafeCritical]
        public static void WriteToFile(string message, params object[] args)
        {
            _logger.TraceFormat(message, args);
        }

        //[Conditional("DEBUG")]
        //[SecuritySafeCritical]
        //public static void TraceStart(string methodName)
        //{
        //    WriteImpl(string.Format("Type start {0}\r\n", methodName));
        //}

        //[Conditional("DEBUG")]
        //[SecuritySafeCritical]
        //public static void TraceEnd(string methodName)
        //{
        //    WriteImpl(string.Format("Type end {0}\r\n", methodName));
        //}

        ////[Conditional("DEBUG")]
        ////[SecuritySafeCritical]
        ////public static void TraceStart(ILGenerator il, string methodName)
        ////{
        ////    Write(il, string.Format("Type start {0}\r\n", methodName));
        ////}

        ////[Conditional("DEBUG")]
        ////[SecuritySafeCritical]
        ////public static void TraceEnd(ILGenerator il, string methodName)
        ////{
        ////    Write(il, string.Format("Type end {0}\r\n", methodName));
        ////}

        ////[Conditional("DEBUG")]
        ////[SecuritySafeCritical]
        ////static void Write(ILGenerator il, string text)
        ////{
        ////    il.Emit(OpCodes.Ldstr, text);
        ////    il.Emit(OpCodes.Call, typeof(DebugTracer).GetMethod("WriteImpl", BindingFlags.Static | BindingFlags.NonPublic));
        ////}

        //[Conditional("DEBUG")]
        //[SecuritySafeCritical]
        //public static void TraceStart(string methodName, long pos)
        //{
        //    WriteImpl(string.Format("Start {0} stream pos {1}\r\n", methodName, pos));
        //}

        //[Conditional("DEBUG")]
        //[SecuritySafeCritical]
        //public static void TraceEnd(string methodName, long pos)
        //{
        //    WriteImpl(string.Format("End {0} stream pos {1}\r\n", methodName, pos));
        //}

        //[Conditional("DEBUG")]
        //[SecuritySafeCritical]
        //static void WriteImpl(string text)
        //{
        //    //const string traceFile = @"D:\trace.txt";
        //    //File.AppendAllText(traceFile, text);
        //}
    }
}

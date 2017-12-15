//
// Authors:
//   刘静谊 (Johnny Liu) <jingeelio@163.com>
//
// Copyright (c) 2017 刘静谊 (Johnny Liu)
//
// Licensed under the LGPLv3 license. Please see <http://www.gnu.org/licenses/lgpl-3.0.html> for license text.
//

using System;
using System.Threading;

#if ENCRYPT
namespace JointCode.Internals
#else
namespace JointCode.Common.Logging
#endif
{
    /// <summary>
    /// DefaultLogger.
    /// </summary>
    class DefaultLogger : ILogger
    {
        readonly string _name;
        readonly LogLevel _defaultLogLevel;
        readonly ILogTarget _logTarget;
        readonly ILogFormatter _logFormatter;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultLogger"/> class.
        /// </summary>
        /// <param name="name">The logger name.</param>
        /// <param name="defaultLogLevel">The default log level.</param>
        /// <param name="logTarget">The log appender.</param>
        /// <param name="logFormatter">The log formatter.</param>
        public DefaultLogger(string name, LogLevel defaultLogLevel, ILogTarget logTarget, ILogFormatter logFormatter)
        {
            _name = name;
            _defaultLogLevel = defaultLogLevel;
            _logTarget = logTarget;
            _logFormatter = logFormatter;
        }

        public string Name
        {
            get { return _name; }
        }

        public void Trace(string message)
        {
            Write(message, LogLevel.Trace);
        }

        public void Trace(Exception exception)
        {
            Write(exception, LogLevel.Trace);
        }

        public void TraceFormat(string message, params object[] args)
        {
            Write(message, LogLevel.Trace, args);
        }

        public void Info(string message)
        {
            Write(message, LogLevel.Info);
        }

        public void Info(Exception exception)
        {
            Write(exception, LogLevel.Info);
        }

        public void InfoFormat(string message, params object[] args)
        {
            Write(message, LogLevel.Info, args);
        }

        public void Debug(string message)
        {
            Write(message, LogLevel.Debug);
        }

        public void Debug(Exception exception)
        {
            Write(exception, LogLevel.Debug);
        }

        public void DebugFormat(string message, params object[] args)
        {
            Write(message, LogLevel.Debug, args);
        }

        public void Warn(string message)
        {
            Write(message, LogLevel.Warn);
        }

        public void Warn(Exception exception)
        {
            Write(exception, LogLevel.Warn);
        }

        public void WarnFormat(string message, params object[] args)
        {
            Write(message, LogLevel.Warn, args);
        }

        public void Error(string message)
        {
            Write(message, LogLevel.Error);
        }

        public void Error(Exception exception)
        {
            Write(exception, LogLevel.Error);
        }

        public void ErrorFormat(string message, params object[] args)
        {
            Write(message, LogLevel.Error, args);
        }

        public void Fatal(string message)
        {
            Write(message, LogLevel.Fatal);
        }

        public void Fatal(Exception exception)
        {
            Write(exception, LogLevel.Fatal);
        }

        public void FatalFormat(string message, params object[] args)
        {
            Write(message, LogLevel.Fatal, args);
        }

        void Write(string message, LogLevel level)
        {
            if (_defaultLogLevel > level) return;
            var logItem = new LogItem 
            { 
                LogLevel = level, LoggerName = _name,
                TimeStamp = DateTime.Now,
                Thread = string.IsNullOrEmpty(Thread.CurrentThread.Name) ? Thread.CurrentThread.ManagedThreadId.ToString() : Thread.CurrentThread.Name,
                AppDomain = AppDomain.CurrentDomain.FriendlyName,
                Message = message
            };
            _logTarget.WriteMessage(_logFormatter.Format(logItem));
        }

        void Write(Exception exception, LogLevel level)
        {
            if (_defaultLogLevel > level) return;
            var logItem = new LogItem
            {
                LogLevel = level,
                LoggerName = _name,
                TimeStamp = DateTime.Now,
                Thread = string.IsNullOrEmpty(Thread.CurrentThread.Name) ? Thread.CurrentThread.ManagedThreadId.ToString() : Thread.CurrentThread.Name,
                AppDomain = AppDomain.CurrentDomain.FriendlyName,
                Exception = exception
            };
            _logTarget.WriteMessage(_logFormatter.Format(logItem));
        }

        void Write(string message, LogLevel level, params object[] args)
        {
            if (_defaultLogLevel > level) return;
            var logItem = new LogItem
            {
                LogLevel = level,
                LoggerName = _name,
                TimeStamp = DateTime.Now,
                Thread = string.IsNullOrEmpty(Thread.CurrentThread.Name) ? Thread.CurrentThread.ManagedThreadId.ToString() : Thread.CurrentThread.Name,
                AppDomain = AppDomain.CurrentDomain.FriendlyName,
                Message = message, 
                Parameters = args
            };
            _logTarget.WriteMessage(_logFormatter.Format(logItem));
        }
    }
}

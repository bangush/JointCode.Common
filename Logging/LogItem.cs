//
// Authors:
//   刘静谊 (Johnny Liu) <jingeelio@163.com>
//
// Copyright (c) 2017 刘静谊 (Johnny Liu)
//
// Licensed under the LGPLv3 license. Please see <http://www.gnu.org/licenses/lgpl-3.0.html> for license text.
//

using System;

namespace JointCode.Common.Logging
{
    /// <summary>
    /// Context information when executes the logging 
    /// </summary>
    public class LogItem
    {
        public string LoggerName { get; internal set; }
        public LogLevel LogLevel { get; internal set; }
        public string Message { get; internal set; }
        public object[] Parameters { get; internal set; } //string.Format(format, parameters)
        public Exception Exception { get; internal set; }
        public DateTime TimeStamp { get; internal set; }
        /// <summary>
        /// Gets the name of the current thread, or the thread id when the name is not available.
        /// </summary>
        public string Thread { get; internal set; }
        public string AppDomain { get; internal set; }
        //public Type Type { get; internal set; }
        //通过配置获取一些环境信息，例如登录用户名称、操作系统磁盘空间情况、StackTrace 等
        //public Dictionary<string, string> Properties { get; internal set; } 
    }
}
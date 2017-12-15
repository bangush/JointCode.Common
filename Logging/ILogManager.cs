//
// Authors:
//   刘静谊 (Johnny Liu) <jingeelio@163.com>
//
// Copyright (c) 2017 刘静谊 (Johnny Liu)
//
// Licensed under the LGPLv3 license. Please see <http://www.gnu.org/licenses/lgpl-3.0.html> for license text.
//

using System;
using System.Collections.Generic;

namespace JointCode.Common.Logging
{
    /// 日志记录可以从 3 个方面考虑：
    /// 1. 可供记录的内容（称为记录项）：这包括用户显示传递的日志信息，还包括一些环境信息，例如操作系统名称和版本、CPU 占用情况等，
    ///    且环境信息应该是可以定制的。
    /// 2. 日志的格式：对于不同的记录项，使用什么格式来组织它们（例如使用空格、制表符或行进行分隔）。
    /// 3. 日志记录的目标：电子邮件、数据库、文本文件等。

    /// <see cref="ILogManager"/> 的目的在于通过配置（可以是代码配置或者通过配置文件进行配置）来实现不同类型的日志记录策略，即能够
    /// 根据不同的类型创建不同的 <see cref="ILogger"/> 进行日志记录。
    public interface ILogManager
    {
        /// <summary>
        /// Gets the default log level. 
        /// This value will be passed down to the <see cref="ILogger"/>. 
        /// Any message passed to the <see cref="ILogger"/> with a log level below this value will not be logged.
        /// </summary>
        /// <remarks>This value can be set programmatically or by configuration file.</remarks>
        LogLevel DefaultLogLevel { get; }

        ILogger GetDefaultLogger();
        ILogger GetLogger(Type type);
        ILogger GetLogger(string loggerName);

        void AddPatternMatcher(IPatternMatcher matcher);
        void AddPatternMatchers(IEnumerable<IPatternMatcher> matchers);
        void RemovePatternMatcher(IPatternMatcher matcher);
    }
}
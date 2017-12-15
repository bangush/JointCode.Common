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

#if ENCRYPT
namespace JointCode.Internals
#else
namespace JointCode.Common.Logging
#endif
{
    class DefaultLogManager : ILogManager
    {
        readonly LogLevel _defaultLogLevel;
        readonly ILogger _logger;

        public DefaultLogManager()
            : this(LogLevel.Trace, new FileLogSetting())
        { }

        public DefaultLogManager(LogLevel defaultLogLevel)
            : this(defaultLogLevel, new FileLogSetting())
        { }

        public DefaultLogManager(FileLogSetting logSetting)
            : this(LogLevel.Trace, logSetting)
        { }

        public DefaultLogManager(LogLevel defaultLogLevel, FileLogSetting logSetting)
        {
            _defaultLogLevel = defaultLogLevel;
            _logger = new DefaultLogger("DefaultLogger", DefaultLogLevel, DefaultFileTarget.GetInstance(logSetting), new DefaultLogFormatter(null));
        }

        public LogLevel DefaultLogLevel
        {
            get { return _defaultLogLevel; }
        }

        public ILogger GetDefaultLogger()
        {
            return _logger;
        }

        public ILogger GetLogger(Type type)
        {
            return new DefaultLogger(type.FullName, DefaultLogLevel, DefaultFileTarget.GetInstance(new FileLogSetting()), new DefaultLogFormatter(null));
        }

        public ILogger GetLogger(string loggerName)
        {
            return new DefaultLogger(loggerName, DefaultLogLevel, DefaultFileTarget.GetInstance(new FileLogSetting()), new DefaultLogFormatter(null));
        }

        public void AddPatternMatcher(IPatternMatcher matcher)
        {
            PatternRepository.Add(matcher);
        }

        public void AddPatternMatchers(IEnumerable<IPatternMatcher> matchers)
        {
            PatternRepository.AddRange(matchers);
        }

        public void RemovePatternMatcher(IPatternMatcher matcher)
        {
            PatternRepository.Remove(matcher);
        }
    }
}

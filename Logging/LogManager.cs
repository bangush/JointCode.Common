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
    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public static class LogManager
    {
        static readonly ILogManager _logManager = new DefaultLogManager();

        public static ILogger GetDefaultLogger()
        {
            return _logManager.GetDefaultLogger();
        }

        public static ILogger GetLogger(Type type)
        {
            return _logManager.GetLogger(type);
        }

        public static ILogger GetLogger(string loggerName)
        {
            return _logManager.GetLogger(loggerName);
        }

        public static void AddPatternMatcher(IPatternMatcher matcher)
        {
            _logManager.AddPatternMatcher(matcher);
        }

        public static void AddPatternMatchers(IEnumerable<IPatternMatcher> matchers)
        {
            _logManager.AddPatternMatchers(matchers);
        }

        public static void RemovePatternMatcher(IPatternMatcher matcher)
        {
            _logManager.RemovePatternMatcher(matcher);
        }
    }
}

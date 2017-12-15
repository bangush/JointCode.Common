//
// Authors:
//   刘静谊 (Johnny Liu) <jingeelio@163.com>
//
// Copyright (c) 2017 刘静谊 (Johnny Liu)
//
// Licensed under the LGPLv3 license. Please see <http://www.gnu.org/licenses/lgpl-3.0.html> for license text.
//

using System;
using JointCode.Common.Extensions;

namespace JointCode.Common.Logging
{
    public interface IPatternMatcher
    {
        /// <summary>
        /// A pattern that uniquely identify the matcher. 
        /// </summary>
        /// <example>
        /// For example: 
        /// timestamp, level, type, space, tab, newline, line, message, ...
        /// </example>
        string Pattern { get; }
        /// <summary>
        /// Determines whether the given pattern string matches the pattern. 
        /// If it matches, build an <see cref="IPatternTranslator"/> and return it.
        /// </summary>
        /// <param name="patternStr">
        /// A pattern string to match with the pattern.
        /// </param>
        /// <param name="translator">The translator.</param>
        /// <example>
        /// For example: 
        /// timestamp, level, type, 3space, 5tab, newline, line, message, ...
        /// </example>
        /// <returns></returns>
        bool TryMatch(string patternStr, out IPatternTranslator translator);
    }

    public abstract class SimplePatternMatcher : IPatternMatcher
    {
        public abstract string Pattern { get; }

        protected abstract IPatternTranslator GetPatternTranslator(string patternStr);

        public bool TryMatch(string patternStr, out IPatternTranslator translator)
        {
            if (Pattern.Equals(patternStr, StringComparison.InvariantCultureIgnoreCase))
            {
                translator = GetPatternTranslator(patternStr);
                return true;
            }
            translator = null;
            return false;
        }
    }

    sealed class NewlinePatternMatcher : SimplePatternMatcher
    {
        static readonly ConstPatternTranslator _translator = new ConstPatternTranslator("\r\n");

        public override string Pattern
        {
            get { return "newline"; }
        }

        protected override IPatternTranslator GetPatternTranslator(string patternStr)
        {
            return _translator;
        }
    }

    sealed class TabPatternMatcher : IPatternMatcher
    {
        static readonly ConstPatternTranslator _translator = new ConstPatternTranslator("\t");

        public string Pattern
        {
            get { return "tab"; }
        }

        public bool TryMatch(string patternStr, out IPatternTranslator translator)
        {
            var index = patternStr.IndexOf(Pattern);
            if (index <= 0)
            {
                translator = null;
                return false;
            }

            var a = patternStr.Substring(0, index);
            int amount;
            if (int.TryParse(a, out amount) || amount <= 0)
            {
                translator = null;
                return false;
            }

            var pad = string.Empty;
            for (int i = 0; i < amount; i++)
                pad += "^t";
            translator = new ConstPatternTranslator(pad);
            return true;
        }
    }

    sealed class TimestampPatternMatcher : SimplePatternMatcher
    {
        sealed class PatternTranslator : IPatternTranslator
        {
            public string Translate(LogItem logItem)
            {
                return logItem.TimeStamp.ToString();
            }
        }

        static readonly PatternTranslator _translator = new PatternTranslator();

        public override string Pattern
        {
            get { return "timestamp"; }
        }

        protected override IPatternTranslator GetPatternTranslator(string patternStr)
        {
            return _translator;
        }
    }

    sealed class LogLevelPatternMatcher : SimplePatternMatcher
    {
        sealed class PatternTranslator : IPatternTranslator
        {
            public string Translate(LogItem logItem)
            {
                return logItem.LogLevel.ToString();
            }
        }

        static readonly PatternTranslator _translator = new PatternTranslator();

        public override string Pattern
        {
            get { return "level"; }
        }

        protected override IPatternTranslator GetPatternTranslator(string patternStr)
        {
            return _translator;
        }
    }

    sealed class ThreadPatternMatcher : SimplePatternMatcher
    {
        sealed class PatternTranslator : IPatternTranslator
        {
            public string Translate(LogItem logItem)
            {
                return logItem.Thread;
            }
        }

        static readonly PatternTranslator _translator = new PatternTranslator();

        public override string Pattern
        {
            get { return "thread"; }
        }

        protected override IPatternTranslator GetPatternTranslator(string patternStr)
        {
            return _translator;
        }
    }

    sealed class MessagePatternMatcher : SimplePatternMatcher
    {
        sealed class PatternTranslator : IPatternTranslator
        {
            public string Translate(LogItem logItem)
            {
                return logItem.Message == null 
                    ? string.Empty 
                    : (logItem.Parameters == null 
                        ? logItem.Message 
                        : string.Format(logItem.Message, logItem.Parameters));
            }
        }

        static readonly PatternTranslator _translator = new PatternTranslator();

        public override string Pattern
        {
            get { return "message"; }
        }

        protected override IPatternTranslator GetPatternTranslator(string patternStr)
        {
            return _translator;
        }
    }

    sealed class ExceptionPatternMatcher : SimplePatternMatcher
    {
        sealed class PatternTranslator : IPatternTranslator
        {
            public string Translate(LogItem logItem)
            {
                return logItem.Exception == null
                    ? string.Empty
                    : "[" + logItem.Exception.GetType().ToTypeNameOnly() + "] " + logItem.Exception.Message;
            }
        }

        static readonly PatternTranslator _translator = new PatternTranslator();

        public override string Pattern
        {
            get { return "exception"; }
        }

        protected override IPatternTranslator GetPatternTranslator(string patternStr)
        {
            return _translator;
        }
    }

    sealed class ExceptionStackTracePatternMatcher : SimplePatternMatcher
    {
        sealed class PatternTranslator : IPatternTranslator
        {
            public string Translate(LogItem logItem)
            {
                return logItem.Exception == null
                    ? string.Empty
                    : Environment.NewLine + logItem.Exception.StackTrace;
            }
        }

        static readonly PatternTranslator _translator = new PatternTranslator();

        public override string Pattern
        {
            get { return "exceptionstacktrace"; }
        }

        protected override IPatternTranslator GetPatternTranslator(string patternStr)
        {
            return _translator;
        }
    }
}
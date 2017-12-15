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
using JointCode.Common.Helpers;
namespace JointCode.Common.Logging
#endif
{
    static class PatternRepository
    {
        static readonly List<IPatternMatcher> _matchers;

        static PatternRepository()
        {
            _matchers = new List<IPatternMatcher>(new IPatternMatcher[]
            {
                new NewlinePatternMatcher(),
                new TimestampPatternMatcher(),
                new TabPatternMatcher(), 
                new LogLevelPatternMatcher(), 
                new ThreadPatternMatcher(), 
                new MessagePatternMatcher(), 
                new ExceptionPatternMatcher(), 
                new ExceptionStackTracePatternMatcher(), 
            });
        }

        internal static bool TryGet(string patternStr, out IPatternTranslator translator)
        {
            lock (_matchers)
            {
                foreach (var matcher in _matchers)
                {
                    if (matcher.TryMatch(patternStr, out translator))
                        return true;
                }
            }
            translator = null;
            return false;
        }

        internal static void Add(IPatternMatcher matcher)
        {
            VerifyParameter(matcher);
            lock (_matchers)
            {
                VerifySymbol(matcher);
                _matchers.Add(matcher);
            }
        }

        internal static void AddRange(IEnumerable<IPatternMatcher> matchers)
        {
            Requires.Instance.NotNull(matchers, "matchers");
            foreach (var matcher in matchers)
                VerifyParameter(matcher);

            lock (_matchers)
            {
                foreach (var matcher in matchers)
                {
                    VerifySymbol(matcher);
                    _matchers.Add(matcher);
                }
            }
        }

        internal static void Remove(IPatternMatcher matcher)
        {
            VerifyParameter(matcher);
            lock (_matchers)
            {
                for (int i = 0; i < _matchers.Count; i++)
                {
                    var m = _matchers[i];
                    if (!ReferenceEquals(m, matcher))
                        continue;
                    _matchers.RemoveAt(i);
                    return;
                }
            }
        }

        static void VerifyParameter(IPatternMatcher matcher)
        {
            Requires.Instance.NotNull(matcher, "matcher");
            Requires.Instance.NotNull(matcher.Pattern, "matcher.Pattern");
        }

        static void VerifySymbol(IPatternMatcher matcher)
        {
            foreach (var m in _matchers)
            {
                if (m.Pattern.Equals(matcher.Pattern, StringComparison.InvariantCultureIgnoreCase))
                    throw new ArgumentException("A matcher with same pattern already exists!");
            }
        }
    }
}

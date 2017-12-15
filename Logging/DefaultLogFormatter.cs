//
// Authors:
//   刘静谊 (Johnny Liu) <jingeelio@163.com>
//
// Copyright (c) 2017 刘静谊 (Johnny Liu)
//
// Licensed under the LGPLv3 license. Please see <http://www.gnu.org/licenses/lgpl-3.0.html> for license text.
//

using System.Collections.Generic;
using System.Text;

#if ENCRYPT
namespace JointCode.Internals
#else
using JointCode.Common.Extensions;
namespace JointCode.Common.Logging
#endif
{
    class DefaultLogFormatter : ILogFormatter
    {
        //The following raw pattern will output {message} or {exception}/{exceptionstacktrace}, determined by which method 
        //of the ILogger interface is called:
        //1. If no exception is passed down, then the {exception}/{exceptionstacktrace} will be ignored.
        //2. If no message is passed down, then the {message} will be ignored.
        //const string DefaultRawPattern = "{timestamp} {thread} {level} - {message}{exception}{newline}";
        const string DefaultRawPattern = "{newline}{timestamp} {thread} {level} - {message}{exception}{exceptionstacktrace}{newline}";
        readonly string _rawPattern;
        readonly IEnumerable<IPatternTranslator> _patternTranslators;

        public DefaultLogFormatter(string rawPattern)
        {
            _rawPattern = rawPattern.IsNullOrWhiteSpace() ? DefaultRawPattern : rawPattern;
            _patternTranslators = PatternParser.Parse(_rawPattern);
        }

        public string RawPattern
        {
            get { return _rawPattern; }
        }

        public string Format(LogItem logItem)
        {
            var message = new StringBuilder();
            foreach (var patternTranslator in _patternTranslators)
            {
                var item = patternTranslator.Translate(logItem);
                message.Append(item);
            }

            return message.ToString();
        }
    }
}

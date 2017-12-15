//
// Authors:
//   刘静谊 (Johnny Liu) <jingeelio@163.com>
//
// Copyright (c) 2017 刘静谊 (Johnny Liu)
//
// Licensed under the LGPLv3 license. Please see <http://www.gnu.org/licenses/lgpl-3.0.html> for license text.
//

namespace JointCode.Common.Logging
{
    public interface IPatternTranslator
    {
        /// <summary>
        /// Extract the logging message from the <paramref name="logItem"/> or runtime context.
        /// </summary>
        /// <param name="logItem">The log item.</param>
        /// <returns></returns>
        string Translate(LogItem logItem);
    }

    public sealed class ConstPatternTranslator : IPatternTranslator
    {
        readonly string _pattern;

        internal ConstPatternTranslator(string pattern)
        {
            _pattern = pattern;
        }

        public string Translate(LogItem logItem)
        {
            return _pattern;
        }
    }
}
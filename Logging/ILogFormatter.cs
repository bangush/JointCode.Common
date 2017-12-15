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
    public interface ILogFormatter
    {
        /// <summary>
        /// Gets the raw pattern. A raw pattern is a string that is composed of multiple pattern strings, and will be used
        /// to create different <see cref="IPatternTranslator"/>s.
        /// </summary>
        /// <example>
        /// For example, the '{China} - {ZhongHua}China' is a raw pattern, which contains 4 pattern strings, and will thus 
        /// produce 4 <see cref="IPatternTranslator"/>s.
        /// </example>
        string RawPattern { get; }
        /// <summary>
        /// Implement this method to create your own layout format.
        /// </summary>
        /// <param name="logItem">The log item.</param>
        /// <returns>
        /// The formatted message.
        /// </returns>
        string Format(LogItem logItem);
    }
}

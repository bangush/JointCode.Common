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
    static class PatternParser
    {
        const char OpeningTag = '{';
        const char ClosingTag = '}';

        struct MatchItem
        {
            /// <summary>
            /// Indicates whether the value is correctly surrounded by the <see cref="OpeningTag"/> and <see cref="ClosingTag"/>.
            /// </summary>
            /// <remarks>
            /// For example, the '{China}' is a tagged value, while the 'China' is not.
            /// </remarks>
            internal bool IsTaggedValue { get; set; }
            /// <summary>
            /// A matched string, even if it is not surrounded by the <see cref="OpeningTag"/> and <see cref="ClosingTag"/>.
            /// </summary>
            /// <remarks>
            /// For example, a raw pattern '{China} - {ZhongHua}China' has 4 <see cref="MatchedString"/>s.
            /// </remarks>
            internal string MatchedString { get; set; }
        }

        /// <summary>
        /// Parses the specified raw pattern to produce appropriate <see cref="IPatternTranslator"/>s.
        /// </summary>
        /// <param name="rawPattern">A raw pattern to be splitted into individual pattern strings, which will be further matched with 
        /// registered patterns to get the <see cref="IPatternTranslator"/>s.</param>
        /// <example>
        /// For example, a raw pattern '{China} - {ZhongHua}China' contains 4 pattern strings, and will thus produce 4 <see cref="IPatternTranslator"/>s.
        /// </example>
        /// <returns></returns>
        public static IEnumerable<IPatternTranslator> Parse(string rawPattern)
        {
            Requires.Instance.NotNullOrWhiteSpace(rawPattern, "rawPattern");

            var matchItems = ParsePattern(rawPattern);
            var patternTranslators = new List<IPatternTranslator>(matchItems.Count);

            foreach (var matchItem in matchItems)
            {
                IPatternTranslator patternTranslator;
                if (matchItem.IsTaggedValue)
                {
                    if (!PatternRepository.TryGet(matchItem.MatchedString, out patternTranslator))
                        patternTranslator = new ConstPatternTranslator(OpeningTag + matchItem.MatchedString + ClosingTag);
                }
                else
                {
                    patternTranslator = new ConstPatternTranslator(matchItem.MatchedString);
                }

                patternTranslators.Add(patternTranslator);
            }

            return patternTranslators;
        }

        static List<MatchItem> ParsePattern(string rawPattern)
        {
            var parsedItem = string.Empty;
            var openTagCount = 0; //执行类似 xml 的标签闭合验证

            var matchedItems = new List<MatchItem>();

            for (int i = 0; i < rawPattern.Length; i++)
            {
                var c = rawPattern[i];
                if (c == OpeningTag)
                {
                    if (openTagCount > 0)
                        throw new ArgumentException("Nested tag is not supported! Invalid raw pattern!"); //标签没有正确闭合，或者尝试嵌套标签

                    if (parsedItem != string.Empty)
                    {
                        matchedItems.Add(new MatchItem{IsTaggedValue = false, MatchedString = parsedItem});
                        parsedItem = string.Empty;
                    }

                    openTagCount += 1;
                }
                else if (c == ClosingTag)
                {
                    openTagCount -= 1;
                    if (parsedItem == string.Empty || openTagCount < 0) //空标签、标签没有正确闭合，或者尝试嵌套标签
                        throw new ArgumentException("Empty tag or nested tag found! Invalid raw pattern!");

                    if (openTagCount == 0)
                    {
                        matchedItems.Add(new MatchItem { IsTaggedValue = true, MatchedString = parsedItem });
                        parsedItem = string.Empty;
                    }
                    else
                    {
                        parsedItem += c;
                    }
                }
                else
                {
                    parsedItem += c;
                }
            }

            if (openTagCount != 0)
                throw new ArgumentException("Invalid raw pattern!"); //标签没有正确闭合，或者尝试嵌套标签

            if (parsedItem != string.Empty)
                matchedItems.Add(new MatchItem { IsTaggedValue = false, MatchedString = parsedItem });
            
            return matchedItems;
        }
    }
}
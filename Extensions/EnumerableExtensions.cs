//
// Authors:
//   刘静谊 (Johnny Liu) <jingeelio@163.com>
//
// Copyright (c) 2017 刘静谊 (Johnny Liu)
//
// Licensed under the LGPLv3 license. Please see <http://www.gnu.org/licenses/lgpl-3.0.html> for license text.
//

using System.Collections.Generic;

#if ENCRYPT
namespace JointCode.Internals
#else
using JointCode.Common.Helpers;
namespace JointCode.Common.Extensions
#endif
{
    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public static class EnumerableExtensions
    {
        public static List<T> ToList<T>(this IEnumerable<T> input)
        {
            Requires.Instance.NotNull(input, "input");
            return new List<T>(input);
        }
    }
}

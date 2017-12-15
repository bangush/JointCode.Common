//
// Authors:
//   刘静谊 (Johnny Liu) <jingeelio@163.com>
//
// Copyright (c) 2017 刘静谊 (Johnny Liu)
//
// Licensed under the LGPLv3 license. Please see <http://www.gnu.org/licenses/lgpl-3.0.html> for license text.
//

using System;

#if ENCRYPT
namespace JointCode.Internals
#else
using JointCode.Common.Helpers;
namespace JointCode.Common.Extensions
#endif
{
    public static class ArrayExtensions
    {
        public static T[] Append<T>(this T[] srcArray, T item)
        {
            Requires.Instance.NotNull(srcArray, "srcArray");
            var newArray = new T[srcArray.Length + 1];
            Array.Copy(srcArray, newArray, srcArray.Length);
            newArray[newArray.Length - 1] = item;
            return newArray;
        }
    }
}

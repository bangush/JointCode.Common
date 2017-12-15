//
// Authors:
//   刘静谊 (Johnny Liu) <jingeelio@163.com>
//
// Copyright (c) 2017 刘静谊 (Johnny Liu)
//
// Licensed under the LGPLv3 license. Please see <http://www.gnu.org/licenses/lgpl-3.0.html> for license text.
//

using System.Collections.Generic;

namespace JointCode.Common.Collections
{
    public interface IReadonlyCollection<T> : IEnumerable<T>
    {
        int Count { get; }
        T this[int i] { get; }
        bool Contains(T item);
        int IndexOf(T item);
    }
}
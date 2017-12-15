//
// Authors:
//   刘静谊 (Johnny Liu) <jingeelio@163.com>
//
// Copyright (c) 2017 刘静谊 (Johnny Liu)
//
// Licensed under the LGPLv3 license. Please see <http://www.gnu.org/licenses/lgpl-3.0.html> for license text.
//

using System;
using System.Collections;
using System.Collections.Generic;

namespace JointCode.Common.Collections
{
    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class ReadonlyQueue<T> : IReadonlyCollection<T>
    {
        readonly CircularBuffer<T> _collection;

        public ReadonlyQueue(CircularBuffer<T> collection)
        {
            _collection = collection;
        }

        public T this[int i]
        {
            get { return _collection[i]; }
        }

        public int Count
        {
            get { return _collection.Size; }
        }

        public bool Contains(T item)
        {
            return _collection.Contains(item);
        }

        public int IndexOf(T item)
        {
            throw new NotImplementedException();
        }

        #region IEnumerable<T> Members

        public IEnumerator<T> GetEnumerator()
        {
            foreach (var item in _collection)
                yield return item;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}
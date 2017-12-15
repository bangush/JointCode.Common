//
// Authors:
//   刘静谊 (Johnny Liu) <jingeelio@163.com>
//
// Copyright (c) 2017 刘静谊 (Johnny Liu)
//
// Licensed under the LGPLv3 license. Please see <http://www.gnu.org/licenses/lgpl-3.0.html> for license text.
//

using System.Collections;
using System.Collections.Generic;

namespace JointCode.Common.Collections
{
    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public partial class ReadonlyList<T> : IReadonlyCollection<T>
    {
        readonly List<T> _collection;

        public ReadonlyList(List<T> collection)
        {
            _collection = collection;
        }

        public ReadonlyList(IEnumerable<T> collection)
        {
            _collection = new List<T>(collection);
        }

        public int Count
        {
            get { return _collection.Count; }
        }

        public T this[int i]
        {
            get { return _collection[i]; }
        }

        public bool Contains(T item)
        {
            return _collection.Contains(item);
        }

        public int IndexOf(T item)
        {
            return _collection.IndexOf(item);
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

    partial class ReadonlyList<T>
    {
        internal void Add(T item)
        {
            _collection.Add(item);
        }

        internal bool Remove(T item)
        {
            return _collection.Remove(item);
        }

        internal void RemoveAt(int index)
        {
            _collection.RemoveAt(index);
        }
    }

    //public class ReadonlyArray<T> : ReadonlyCollection<T, T[]>
    //{
    //    public ReadonlyArray(T[] array) : base(array) { }
    //}
}

//// -----------------------------------------------------------------------------
//// This source file is part of Matrix Platform
//// 	(Universal .NET Software Development Platform)
//// For the latest info, see http://www.matrixplatform.com
//// 
//// Copyright (c) 2009-2010, Ingenious Ltd
//// 
//// Permission is hereby granted, free of charge, to any person obtaining a copy
//// of this software and associated documentation files (the "Software"), to deal
//// in the Software without restriction, including without limitation the rights
//// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//// copies of the Software, and to permit persons to whom the Software is
//// furnished to do so, subject to the following conditions:
//// 
//// The above copyright notice and this permission notice shall be included in
//// all copies or substantial portions of the Software.
//// 
//// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
//// THE SOFTWARE.
//// -----------------------------------------------------------------------------
//using System;
//using System.Collections.Generic;
//using System.Collections.ObjectModel;

//namespace JointCode.Common.Collections
//{
//    /// <summary>
//    /// Hot swapping.
//    /// 
//    /// To evade locking - when adding new items, simply replace the list with a new one.
//    /// </summary>
//    [Serializable]
//    public class HotSwapList<TType> : IList<TType>
//    {
//        volatile List<TType> _items = new List<TType>();

//        public List<TType> CurrentInstance
//        {
//            get
//            {
//                return _items;
//            }
//        }

//        /// <summary>
//        /// Constructor.
//        /// </summary>
//        public HotSwapList()
//        {
//        }

//        /// <summary>
//        /// This allows to quickly capture a read only version of the 
//        /// collection that can be further used as needed in any scenario.
//        /// </summary>
//        /// <returns></returns>
//        public ReadOnlyCollection<TType> AsReadOnly()
//        {
//            return _items.AsReadOnly();
//        }

//        /// <summary>
//        /// Add item only if it does not already exist.
//        /// </summary>
//        /// <param name="item"></param>
//        /// <returns>True if the add was performed, or false if it already exists.</returns>
//        public bool AddUnique(TType item)
//        {
//            lock (this)
//            {
//                if (_items.Contains(item))
//                {
//                    return false;
//                }

//                Add(item);
//            }

//            return true;
//        }

//        /// <summary>
//        /// Try to obtain a value with this index, return false if we fail and no modification to value done.
//        /// </summary>
//        /// <param name="index">The index of the item retrieved.</param>
//        /// <param name="value">The resulting retrieve value.</param>
//        /// <returns>True if the value was retrieved, otherwise false.</returns>
//        public bool TryGetValue(int index, ref TType value)
//        {
//            List<TType> instance = _items;
//            if (instance.Count > index)
//            {
//                value = instance[index];
//                return true;
//            }

//            return false;
//        }

//        /// <summary>
//        /// Sort the items in the collection (must implement IComparable).
//        /// </summary>
//        public bool Sort()
//        {
//            if (!typeof(IComparable).IsAssignableFrom(typeof(TType)))
//                return false;

//            lock (this)
//            {
//                var items = new SortedList<TType, TType>();
//                foreach (TType item in _items)
//                {
//                    items.Add(item, item);
//                }

//                SetToRange(EnumerableToArray(items.Values));
//            }

//            return true;
//        }

//        /// <summary>
//        /// Helper, converts enumerable items collection into an array of the same.
//        /// </summary>
//        static TDataType[] EnumerableToArray<TDataType>(IEnumerable<TDataType> enumerable)
//        {
//            List<TDataType> list = new List<TDataType>();
//            foreach (TDataType value in enumerable)
//            {
//                list.Add(value);
//            }
//            return list.ToArray();
//        }

//        /// <summary>
//        /// Clear all items and add the current badge.
//        /// </summary>
//        public void SetToRange(IEnumerable<TType> items)
//        {
//            lock (this)
//            {
//                List<TType> instance = new List<TType>();
//                instance.AddRange(items);
//                _items = instance;
//            }
//        }

//        public void AddRange(IEnumerable<TType> items)
//        {
//            lock (this)
//            {
//                List<TType> instance = new List<TType>(_items);
//                instance.AddRange(items);
//                _items = instance;
//            }
//        }
        
//        /// <summary>
//        /// Remove all instances that are equal, or the same as, this item.
//        /// </summary>
//        /// <returns>Count of items removed.</returns>
//        public int RemoveAll(TType item)
//        {
//            int result = 0;
//            lock (this)
//            {
//                List<TType> instance = new List<TType>(_items);
                
//                while (instance.Remove(item))
//                {
//                    result++;
//                }

//                if (result != 0)
//                {
//                    _items = instance;
//                }
//            }

//            return result;
//        }


//        #region IList<TType> Members

//        public int IndexOf(TType item)
//        {
//            return _items.IndexOf(item);
//        }

//        public void Insert(int index, TType item)
//        {
//            lock (this)
//            {
//                List<TType> items = new List<TType>(_items);
//                items.Insert(index, item);
//                _items = items;
//            }
//        }

//        /// <summary>
//        /// Implementation has internal check security,
//        /// so no exceptions occur.
//        /// </summary>
//        /// <param name="index"></param>
//        public void RemoveAt(int index)
//        {
//            lock (this)
//            {
//                if (_items.Count > index)
//                {
//                    List<TType> items = new List<TType>(_items);
//                    items.RemoveAt(index);
//                    _items = items;
//                }
//            }
//        }

//        /// <summary>
//        /// *Warning* setting a value if very slow, since it redoes the hotswaps
//        /// the entire collection too, so use with caution.
//        /// </summary>
//        /// <param name="index"></param>
//        /// <returns></returns>
//        public TType this[int index]
//        {
//            get
//            {
//                return _items[index];
//            }

//            set
//            {
//                lock (this)
//                {
//                    List<TType> items = new List<TType>(_items);
//                    items[index] = value;
//                    _items = items;
//                }
//            }
//        }

//        #endregion

//        #region ICollection<TType> Members

//        public void Add(TType item)
//        {
//            lock (this)
//            {
//                List<TType> items = new List<TType>(_items);
//                items.Add(item);
//                _items = items;
//            }
//        }

//        public bool Remove(TType item)
//        {
//            lock (this)
//            {
//                if (_items.Contains(item) == false)
//                {
//                    return false;
//                }

//                List<TType> items = new List<TType>(_items);
//                items.Remove(item);
//                _items = items;
//            }

//            return true;
//        }

//        public void Clear()
//        {
//            lock (this)
//            {
//                _items = new List<TType>();
//            }
//        }

//        public bool Contains(TType item)
//        {
//            return _items.Contains(item);
//        }

//        public void CopyTo(TType[] array, int arrayIndex)
//        {
//            _items.CopyTo(array, arrayIndex);
//        }

//        public int Count
//        {
//            get { return _items.Count; }
//        }

//        public bool IsReadOnly
//        {
//            get { return false; }
//        }


//        #endregion

//        #region IEnumerable<TType> Members

//        public IEnumerator<TType> GetEnumerator()
//        {
//            return _items.GetEnumerator();
//        }

//        #endregion

//        #region IEnumerable Members

//        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
//        {
//            return _items.GetEnumerator();
//        }

//        #endregion
//    }
//}

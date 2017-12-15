using System;
using System.Collections;
using System.Collections.Generic;

#if ENCRYPT
namespace JointCode.Internals
#else
namespace JointCode.Common.Collections
#endif
{
    internal struct _ExtendedDictionaryNode<TKey, TValue>
    {
        internal int _nextNodeIndex;
        internal int _hashCode;
        internal TKey _key;
        internal TValue _value;
    }

    /// <summary>
    /// This is a dictionary implementation that I made that has an speed comparable
    /// to the .NET dictionary but also has extra methods, like GetOrCreateValue, GetOrAdd
    /// which do two actions at once, avoiding two searches in the dictionary (like
    /// a ContainsKey and then Add) so it can give better performance if such
    /// methods are used.
    /// Also, it is simpler in structure (less responsibilities) and does not implement 
    /// the IDictionary interface.
    /// Its Keys and Values are simple IEnumerables, not ICollections, and it also
    /// does not implements serialization (but serialization can still be done by a 
    /// good framework).
    /// It can also be Trimmed to reduce the capacity (and memory consuption) when you 
    /// know you finished adding items to it.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys used by this dictionary.</typeparam>
    /// <typeparam name="TValue">The type of the values stored in this dictionary.</typeparam>
    public sealed class HashMap<TKey, TValue> :
        IHashMap<TKey, TValue>,
        IHashMap
    {
        #region Private Fields - Property specific fields are with the property
        int[] _indexes;
        // struct数组在物理上是一个连续的内存块，因此像下面这样的数组元素读写操作可以利用 cpu 的高速缓存，
        // 而不会由于反复通过下标访问数组而造成性能损失：
        // newNodes[newNodeIndex]._hashCode = hashCode;
        // newNodes[newNodeIndex]._key = oldNodes[oldNodeIndex]._key;
        // 此外，对struct数组元素的下标访问不会造成复制（List元素的下标访问则会），直接内存定位效率很高
        _ExtendedDictionaryNode<TKey, TValue>[] _nodes; 
        int _indexOfFirstFree = -1;
        #endregion

        #region Constructors
        /// <summary>
        /// Creates a dictionary with its default capacity and key comparer.
        /// </summary>
        public HashMap() :
            this(31, null)
        { }

        /// <summary>
        /// Creates a dictionary with the given capacity and the default key comparer.
        /// </summary>
        public HashMap(int capacity) :
            this(capacity, null)
        { }

        /// <summary>
        /// Creates a new dictionary with the given key comparer and using the
        /// default capacity.
        /// </summary>
        public HashMap(IEqualityComparer<TKey> comparer) :
                this(31, comparer)
        { }

        /// <summary>
        /// Creates a new dictionary with the given capacity and key comparer.
        /// </summary>
        /// <param name="capacity">The capacity to use.</param>
        /// <param name="comparer">The comparer to use for the keys. A value of null means the default one will be used.</param>
        public HashMap(int capacity, IEqualityComparer<TKey> comparer)
        {
            capacity = DictionaryHelper.AdaptLength(capacity);

            _indexes = new int[capacity];
            _nodes = new _ExtendedDictionaryNode<TKey, TValue>[capacity];

            if (comparer == null)
                comparer = EqualityComparer<TKey>.Default;

            _comparer = comparer;
            int length = _indexes.Length;
            for (int i = 0; i < length; i++)
                _indexes[i] = -1;
        }
        #endregion

        #region Properties
        #region this[]
        /// <summary>
        /// Gets or sets a value for a given key.
        /// If you are getting the value for a key, it should exists or an
        /// ArgumentException exception will be thrown.
        /// </summary>
        public TValue this[TKey key]
        {
            get
            {
                if (key == null)
                    throw new ArgumentNullException("key");

                int hashCode = _comparer.GetHashCode(key) & int.MaxValue;
                int bucketIndex = hashCode % _indexes.Length;
                int nodeIndex = _indexes[bucketIndex];

                while (nodeIndex != -1)
                {
                    if (_nodes[nodeIndex]._hashCode == hashCode)
                        if (_comparer.Equals(_nodes[nodeIndex]._key, key))
                            return _nodes[nodeIndex]._value;

                    nodeIndex = _nodes[nodeIndex]._nextNodeIndex;
                }

                throw new ArgumentException("The is no item with the given key: " + key, "key");
            }
            set
            {
                TValue oldValue;
                TryGetAndAddOrReplace(key, value, out oldValue);
            }
        }
        #endregion

        #region Capacity
        /// <summary>
        /// Gets the Capacity of this dictionary. That is, the size of the actual
        /// buffer. So, if there is a difference between Count and Capacity, new
        /// items can be added without reallocations.
        /// </summary>
        public int Capacity
        {
            get { return _indexes.Length; }
        }
        #endregion
        #region Comparer
        readonly IEqualityComparer<TKey> _comparer;
        /// <summary>
        /// Gets the comparer used by this dictionary. Even if you didn't set one.
        /// In such case, the EqualityComparer&lt;&gt;.Default will be returned.
        /// </summary>
        public IEqualityComparer<TKey> Comparer
        {
            get { return _comparer; }
        }
        #endregion
        #region Count
        int _count;
        /// <summary>
        /// Gets the actual number of items in this dictionary.
        /// </summary>
        public int Count
        {
            get { return _count; }
        }
        #endregion
        #region Keys
        /// <summary>
        /// Enumerates all the keys present in this dictionary.
        /// </summary>
        public IEnumerable<TKey> Keys
        {
            get
            {
                foreach (int firstNodeIndex in _indexes)
                {
                    int nodeIndex = firstNodeIndex;
                    while (nodeIndex != -1)
                    {
                        yield return _nodes[nodeIndex]._key;

                        nodeIndex = _nodes[nodeIndex]._nextNodeIndex;
                    }
                }
            }
        }
        #endregion
        #region Values
        /// <summary>
        /// Enumerates all the values present in this dictionary.
        /// </summary>
        public IEnumerable<TValue> Values
        {
            get
            {
                foreach (int firstNodeIndex in _indexes)
                {
                    int nodeIndex = firstNodeIndex;
                    while (nodeIndex != -1)
                    {
                        yield return _nodes[nodeIndex]._value;

                        nodeIndex = _nodes[nodeIndex]._nextNodeIndex;
                    }
                }
            }
        }
        #endregion
        #endregion
        #region Methods
        #region _Grow
        void _Grow()
        {
            int bucketsLength = _indexes.Length;

            int newLength = bucketsLength * 2;
            SetCapacity(newLength);
        }
        #endregion

        #region TrimExcess
        /// <summary>
        /// Removes any exceeding space (Capacity) in this dictionary.
        /// This action may return false if there is no need to reduce the capacity.
        /// An OutOfMemoryException may also be thrown, as new memory is allocated
        /// and the data is copied before really reducing the used memory. But,
        /// in such situation the data is kept intact.
        /// </summary>
        public bool TrimExcess()
        {
            return SetCapacity(_count);
        }
        #endregion
        #region SetCapacity
        /// <summary>
        /// Sets the capacity of the dictionary a value that is at least as big
        /// as the given value. The real capacity may be different (usually bigger)
        /// because of the internal calculation done to have better hashing distribution.
        /// </summary>
        public bool SetCapacity(int value)
        {
            int count = _count;
            if (value < count)
                throw new ArgumentOutOfRangeException("value", "Can't set a capacity that is not enough to hold all the actual items (Count=" + count + ", value=" + value + ").");

            int newLength = DictionaryHelper.AdaptLength(value);
            if (newLength == _indexes.Length)
                return false;

            var newNodes = new _ExtendedDictionaryNode<TKey, TValue>[newLength];
            var newIndexes = new int[newLength];
            for (int i = 0; i < newLength; i++)
                newIndexes[i] = -1;

            var oldNodes = _nodes;
            var oldIndexes = _indexes;
            int newNodeIndex = -1;
            foreach (int firstNodeIndex in oldIndexes)
            {
                int oldNodeIndex = firstNodeIndex;
                while (oldNodeIndex != -1)
                {
                    int hashCode = oldNodes[oldNodeIndex]._hashCode;
                    int newBucketIndex = hashCode % newLength;

                    newNodeIndex++;
                    newNodes[newNodeIndex]._hashCode = hashCode;
                    newNodes[newNodeIndex]._key = oldNodes[oldNodeIndex]._key;
                    newNodes[newNodeIndex]._nextNodeIndex = newIndexes[newBucketIndex];
                    newNodes[newNodeIndex]._value = oldNodes[oldNodeIndex]._value;
                    newIndexes[newBucketIndex] = newNodeIndex;

                    oldNodeIndex = oldNodes[oldNodeIndex]._nextNodeIndex;
                }
            }

            _indexOfFirstFree = -1;
            _indexes = newIndexes;
            _nodes = newNodes;
            return true;
        }
        #endregion
        #region Clear
        /// <summary>
        /// Removes all the items in this dictionary.
        /// Note that doing this does not change the Capacity of the dictionary.
        /// </summary>
        public void Clear()
        {
            _count = 0;
            _indexOfFirstFree = -1;

            if (!typeof(TKey).IsPrimitive || !typeof(TValue).IsPrimitive)
            {
                foreach (int firstNodeIndex in _indexes)
                {
                    int nodeIndex = firstNodeIndex;
                    while (nodeIndex != -1)
                    {
                        if (!typeof(TKey).IsPrimitive)
                            _nodes[nodeIndex]._key = default(TKey);

                        if (!typeof(TValue).IsPrimitive)
                            _nodes[nodeIndex]._value = default(TValue);

                        nodeIndex = _nodes[nodeIndex]._nextNodeIndex;
                    }
                }
            }

            int count = _indexes.Length;
            for (int i = 0; i < count; i++)
                _indexes[i] = -1;
        }
        #endregion
        #region ContainsKey
        /// <summary>
        /// Verifies if an item with the given key is present on the dictionary.
        /// </summary>
        public bool ContainsKey(TKey key)
        {
            if (key == null)
                throw new ArgumentNullException("key");

            int hashCode = _comparer.GetHashCode(key) & int.MaxValue;
            int bucketIndex = hashCode % _indexes.Length;
            int nodeIndex = _indexes[bucketIndex];

            while (nodeIndex != -1)
            {
                if (_nodes[nodeIndex]._hashCode == hashCode)
                    if (_comparer.Equals(_nodes[nodeIndex]._key, key))
                        return true;

                nodeIndex = _nodes[nodeIndex]._nextNodeIndex;
            }

            return false;
        }
        #endregion
        #region TryGetValue
        /// <summary>
        /// Tries to get a value for the given key.
        /// If the value exists, the result is true and the out value will
        /// contain it.
        /// If the value does not exist, the return is false and value
        /// will have the default(TValue).
        /// </summary>
        public bool TryGetValue(TKey key, out TValue value)
        {
            if (key == null)
                throw new ArgumentNullException("key");

            int hashCode = _comparer.GetHashCode(key) & int.MaxValue;
            int bucketIndex = hashCode % _indexes.Length;
            int nodeIndex = _indexes[bucketIndex];

            while (nodeIndex != -1)
            {
                if (_nodes[nodeIndex]._hashCode == hashCode)
                {
                    if (_comparer.Equals(_nodes[nodeIndex]._key, key))
                    {
                        value = _nodes[nodeIndex]._value;
                        return true;
                    }
                }

                nodeIndex = _nodes[nodeIndex]._nextNodeIndex;
            }

            value = default(TValue);
            return false;
        }
        #endregion
        #region GetValueOrDefault
        /// <summary>
        /// Gets a value for the given key or returns the default(TValue).
        /// This is specially useful for reference types, as the default is null
        /// and usually it is never added to the dictionary.
        /// </summary>
        public TValue GetValueOrDefault(TKey key)
        {
            if (key == null)
                throw new ArgumentNullException("key");

            int hashCode = _comparer.GetHashCode(key) & int.MaxValue;
            int bucketIndex = hashCode % _indexes.Length;
            int nodeIndex = _indexes[bucketIndex];

            while (nodeIndex != -1)
            {
                if (_nodes[nodeIndex]._hashCode == hashCode)
                    if (_comparer.Equals(_nodes[nodeIndex]._key, key))
                        return _nodes[nodeIndex]._value;

                nodeIndex = _nodes[nodeIndex]._nextNodeIndex;
            }

            return default(TValue);
        }
        #endregion
        #region GetOrCreate
        /// <summary>
        /// Tries to get a value for the given key. If it does not exist,
        /// the createValue delegate will be invoked, the value will
        /// be added to the dictionary and returned.
        /// Note that this is usually faster than calling ContainsKey and Add,
        /// as a single search will be done.
        /// </summary>
        public TValue GetOrCreate(TKey key, MyFunc<TKey, TValue> createValue)
        {
            if (key == null)
                throw new ArgumentNullException("key");

            int hashCode = _comparer.GetHashCode(key) & int.MaxValue;
            int bucketIndex = hashCode % _indexes.Length;
            int storedOldNodeIndex = _indexes[bucketIndex];
            int oldNodeIndex = storedOldNodeIndex;

            while (oldNodeIndex != -1)
            {
                if (_nodes[oldNodeIndex]._hashCode == hashCode)
                    if (_comparer.Equals(_nodes[oldNodeIndex]._key, key))
                        return _nodes[oldNodeIndex]._value;

                oldNodeIndex = _nodes[oldNodeIndex]._nextNodeIndex;
            }

            int nextFree = _indexOfFirstFree;
            if (nextFree != -1)
                _indexOfFirstFree = _nodes[nextFree]._nextNodeIndex;
            else
            {
                if (_count == _indexes.Length)
                {
                    _Grow();
                    bucketIndex = hashCode % _indexes.Length;
                    storedOldNodeIndex = _indexes[bucketIndex];
                }

                nextFree = _count;
            }

            TValue result = createValue(key);
            _nodes[nextFree]._hashCode = hashCode;
            _nodes[nextFree]._key = key;
            _nodes[nextFree]._value = result;
            _nodes[nextFree]._nextNodeIndex = storedOldNodeIndex;

            _indexes[bucketIndex] = nextFree;
            _count++;
            return result;
        }
        #endregion
        #region GetOrAdd
        /// <summary>
        /// Gets the value for the given key or adds the value being given.
        /// Returns true if a value already existed, false if it was added now.
        /// </summary>
        public bool GetOrAdd(TKey key, TValue value, out TValue result)
        {
            if (key == null)
                throw new ArgumentNullException("key");

            int hashCode = _comparer.GetHashCode(key) & int.MaxValue;
            int bucketIndex = hashCode % _indexes.Length;
            int storedOldNodeIndex = _indexes[bucketIndex];
            int oldNodeIndex = storedOldNodeIndex;

            while (oldNodeIndex != -1)
            {
                if (_nodes[oldNodeIndex]._hashCode == hashCode)
                {
                    if (_comparer.Equals(_nodes[oldNodeIndex]._key, key))
                    {
                        result = _nodes[oldNodeIndex]._value;
                        return true;
                    }
                }

                oldNodeIndex = _nodes[oldNodeIndex]._nextNodeIndex;
            }

            int nextFree = _indexOfFirstFree;
            if (nextFree != -1)
                _indexOfFirstFree = _nodes[nextFree]._nextNodeIndex;
            else
            {
                if (_count == _indexes.Length)
                {
                    _Grow();
                    bucketIndex = hashCode % _indexes.Length;
                    storedOldNodeIndex = _indexes[bucketIndex];
                }

                nextFree = _count;
            }

            _nodes[nextFree]._hashCode = hashCode;
            _nodes[nextFree]._key = key;
            _nodes[nextFree]._value = value;
            _nodes[nextFree]._nextNodeIndex = storedOldNodeIndex;

            _indexes[bucketIndex] = nextFree;
            _count++;

            result = value;
            return false;
        }
        #endregion
        #region Remove
        /// <summary>
        /// Tries to remove an item.
        /// The return is true if the item existed and was removed.
        /// The return is false if the item didn't exist.
        /// </summary>
        public bool Remove(TKey key)
        {
            if (key == null)
                throw new ArgumentNullException("key");

            int hashCode = _comparer.GetHashCode(key) & int.MaxValue;
            int bucketIndex = hashCode % _indexes.Length;
            int nodeIndex = _indexes[bucketIndex];

            int previousNodeIndex = -1;
            while (nodeIndex != -1)
            {
                if (_nodes[nodeIndex]._hashCode == hashCode)
                {
                    if (_comparer.Equals(_nodes[nodeIndex]._key, key))
                    {
                        if (previousNodeIndex == -1)
                            _indexes[bucketIndex] = _nodes[nodeIndex]._nextNodeIndex;
                        else
                            _nodes[previousNodeIndex]._nextNodeIndex = _nodes[nodeIndex]._nextNodeIndex;

                        if (!typeof(TKey).IsPrimitive)
                            _nodes[nodeIndex]._key = default(TKey);

                        if (!typeof(TValue).IsPrimitive)
                            _nodes[nodeIndex]._value = default(TValue);

                        _nodes[nodeIndex]._nextNodeIndex = _indexOfFirstFree;
                        _indexOfFirstFree = nodeIndex;
                        _count--;
                        return true;
                    }
                }

                previousNodeIndex = nodeIndex;
                nodeIndex = _nodes[nodeIndex]._nextNodeIndex;
            }

            return false;
        }
        #endregion
        #region TryGetValueAndRemove
        /// <summary>
        /// Tries to get a value and also remove it from the dictionary.
        /// If an item with the given key existed, the return is true and
        /// you can check the old stored value that will now be stored in the
        /// out value.
        /// If the return is value, an item was not found and so, there was
        /// nothing to remove and the value will be the default(TValue).
        /// </summary>
        public bool TryGetAndRemove(TKey key, out TValue value)
        {
            if (key == null)
                throw new ArgumentNullException("key");

            int hashCode = _comparer.GetHashCode(key) & int.MaxValue;
            int bucketIndex = hashCode % _indexes.Length;
            int nodeIndex = _indexes[bucketIndex];

            int previousNodeIndex = -1;
            while (nodeIndex != -1)
            {
                if (_nodes[nodeIndex]._hashCode == hashCode)
                {
                    if (_comparer.Equals(_nodes[nodeIndex]._key, key))
                    {
                        if (previousNodeIndex == -1)
                            _indexes[bucketIndex] = _nodes[nodeIndex]._nextNodeIndex;
                        else
                            _nodes[previousNodeIndex]._nextNodeIndex = _nodes[nodeIndex]._nextNodeIndex;

                        if (!typeof(TKey).IsPrimitive)
                            _nodes[nodeIndex]._key = default(TKey);

                        value = _nodes[nodeIndex]._value;
                        if (!typeof(TValue).IsPrimitive)
                            _nodes[nodeIndex]._value = default(TValue);

                        _nodes[nodeIndex]._nextNodeIndex = _indexOfFirstFree;
                        _indexOfFirstFree = nodeIndex;
                        _count--;
                        return true;
                    }
                }

                previousNodeIndex = nodeIndex;
                nodeIndex = _nodes[nodeIndex]._nextNodeIndex;
            }

            value = default(TValue);
            return false;
        }
        #endregion
        #region GetValueAndRemove
        /// <summary>
        /// Removes an item with the given key and returns the value that was there.
        /// If an item with the given key doesn't exist, an ArgumentException is thrown.
        /// </summary>
        public TValue GetAndRemove(TKey key)
        {
            TValue result;
            if (TryGetAndRemove(key, out result))
                return result;

            throw new ArgumentException("There is no item with the given key in this dictionary.");
        }
        #endregion
        #region TryAdd
        /// <summary>
        /// Tries to add an item with the given key.
        /// If an item with the same key already exists, the value is not replaced and
        /// the result is false. If the item is correctly added, the return is true.
        /// </summary>
        public bool TryAdd(TKey key, TValue value)
        {
            TValue oldValue;
            return !GetOrAdd(key, value, out oldValue);
        }
        #endregion
        #region Add
        /// <summary>
        /// Adds an item with the given key. If another item with the same key
        /// already exists, an ArgumentException is thrown.
        /// </summary>
        public void Add(TKey key, TValue value)
        {
            if (!TryAdd(key, value))
                throw new ArgumentException("An item with the same key (" + key + ") already exists.");
        }
        #endregion
        #region TryGetValueOrAdd
        /// <summary>
        /// Tries to get a value for a given key in this dictionary. If one is not found,
        /// adds the given value.
        /// Returns true if a value was found for the key, false if the value was created.
        /// If the value was added, the oldValue should always be default(TValue).
        /// If you simple want to get a value, be it old or new, use GetOrAdd.
        /// </summary>
        public bool TryGetOrAdd(TKey key, TValue value, out TValue oldValue)
        {
            if (key == null)
                throw new ArgumentNullException("key");

            int hashCode = _comparer.GetHashCode(key) & int.MaxValue;
            int bucketIndex = hashCode % _indexes.Length;
            int storedOldNodeIndex = _indexes[bucketIndex];
            int oldNodeIndex = storedOldNodeIndex;

            while (oldNodeIndex != -1)
            {
                if (_nodes[oldNodeIndex]._hashCode == hashCode)
                {
                    if (_comparer.Equals(_nodes[oldNodeIndex]._key, key))
                    {
                        oldValue = _nodes[oldNodeIndex]._value;
                        return true;
                    }
                }

                oldNodeIndex = _nodes[oldNodeIndex]._nextNodeIndex;
            }

            int nextFree = _indexOfFirstFree;
            if (nextFree != -1)
                _indexOfFirstFree = _nodes[nextFree]._nextNodeIndex;
            else
            {
                if (_count == _indexes.Length)
                {
                    _Grow();
                    bucketIndex = hashCode % _indexes.Length;
                    storedOldNodeIndex = _indexes[bucketIndex];
                }

                nextFree = _count;
            }

            _nodes[nextFree]._hashCode = hashCode;
            _nodes[nextFree]._key = key;
            _nodes[nextFree]._value = value;
            _nodes[nextFree]._nextNodeIndex = storedOldNodeIndex;

            _indexes[bucketIndex] = nextFree;
            _count++;

            oldValue = default(TValue);
            return false;
        }
        #endregion
        #region TryGetValueAndAddOrReplace
        /// <summary>
        /// Adds or replaces a value from this dictionary and also returns the oldValue that was
        /// there.
        /// Returns true if there was an oldValue or false if there was not. In such case,
        /// oldValue will contain the default(TValue).
        /// 
        /// This method is useful if you should dispose the old value.
        /// </summary>
        public bool TryGetAndAddOrReplace(TKey key, TValue value, out TValue oldValue)
        {
            if (key == null)
                throw new ArgumentNullException("key");

            int hashCode = _comparer.GetHashCode(key) & int.MaxValue;
            int bucketIndex = hashCode % _indexes.Length;
            int storedOldNodeIndex = _indexes[bucketIndex];
            int oldNodeIndex = storedOldNodeIndex;

            while (oldNodeIndex != -1)
            {
                if (_nodes[oldNodeIndex]._hashCode == hashCode)
                {
                    if (_comparer.Equals(_nodes[oldNodeIndex]._key, key))
                    {
                        oldValue = _nodes[oldNodeIndex]._value;
                        _nodes[oldNodeIndex]._value = value;
                        return true;
                    }
                }

                oldNodeIndex = _nodes[oldNodeIndex]._nextNodeIndex;
            }

            int nextFree = _indexOfFirstFree;
            if (nextFree != -1)
                _indexOfFirstFree = _nodes[nextFree]._nextNodeIndex;
            else
            {
                if (_count == _indexes.Length)
                {
                    _Grow();
                    bucketIndex = hashCode % _indexes.Length;
                    storedOldNodeIndex = _indexes[bucketIndex];
                }

                nextFree = _count;
            }

            _nodes[nextFree]._hashCode = hashCode;
            _nodes[nextFree]._key = key;
            _nodes[nextFree]._value = value;
            _nodes[nextFree]._nextNodeIndex = storedOldNodeIndex;

            _indexes[bucketIndex] = nextFree;
            _count++;

            oldValue = default(TValue);
            return false;
        }
        #endregion

        #region AddMany
        /// <summary>
        /// Adds many items at once.
        /// If an item already exists, an exception is thrown but previous items are
        /// already added.
        /// </summary>
        public void AddMany(ICollection<KeyValuePair<TKey, TValue>> pairs)
        {
            if (pairs == null)
                throw new ArgumentNullException("pairs");

            int pairCount = pairs.Count;
            if (pairCount == 0)
                return;

            int newCount = _count + pairCount;
            if (newCount > Capacity)
                SetCapacity(Math.Max(newCount, Capacity * 2));

            foreach (var pair in pairs)
                Add(pair.Key, pair.Value);
        }
        #endregion
        #region AddOrReplaceMany
        /// <summary>
        /// Adds or replaces many items at once.
        /// </summary>
        public void AddOrReplaceMany(IEnumerable<KeyValuePair<TKey, TValue>> pairs)
        {
            if (pairs == null)
                throw new ArgumentNullException("pairs");

            bool isEmpty = true;
            int itemsToAdd = 0;
            foreach (var pair in pairs)
            {
                isEmpty = false;

                if (!ContainsKey(pair.Key))
                    itemsToAdd++;
            }

            if (isEmpty)
                return;

            int newCount = _count + itemsToAdd;
            if (newCount > Capacity)
                SetCapacity(Math.Max(newCount, Capacity * 2));

            TValue oldValue;
            foreach (var pair in pairs)
                TryGetAndAddOrReplace(pair.Key, pair.Value, out oldValue);
        }
        #endregion
        #region RemoveMany
        /// <summary>
        /// Removes multiple items at once.
        /// </summary>
        public void RemoveMany(RemoveManyDictionaryDelegate<TKey, TValue> checkDelegate)
        {
            if (checkDelegate == null)
                throw new ArgumentNullException("checkDelegate");

            int count = _indexes.Length;
            for (int i = 0; i < count; i++)
            {
                int lastNodeIndex = -1;

                int nodeIndex = _indexes[i];
                while (nodeIndex != -1)
                {
                    TKey key = _nodes[nodeIndex]._key;
                    TValue value = _nodes[nodeIndex]._value;

                    if (!checkDelegate(key, value))
                    {
                        lastNodeIndex = nodeIndex;
                        nodeIndex = _nodes[nodeIndex]._nextNodeIndex;
                        continue;
                    }

                    int nextNodeIndex = _nodes[nodeIndex]._nextNodeIndex;
                    if (lastNodeIndex == -1)
                        _indexes[i] = nextNodeIndex;
                    else
                        _nodes[lastNodeIndex]._nextNodeIndex = nextNodeIndex;

                    if (!typeof(TKey).IsPrimitive)
                        _nodes[nodeIndex]._key = default(TKey);

                    if (!typeof(TValue).IsPrimitive)
                        _nodes[nodeIndex]._value = default(TValue);

                    _nodes[nodeIndex]._nextNodeIndex = _indexOfFirstFree;
                    _indexOfFirstFree = nodeIndex;

                    _count--;
                    nodeIndex = nextNodeIndex;
                }
            }
        }
        #endregion

        #region GetEnumerator
        /// <summary>
        /// Gets an enumerator with key/value pairs that you can use to
        /// check all the existing items in this dictionary.
        /// </summary>
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            foreach (int firstNodeIndex in _indexes)
            {
                int nodeIndex = firstNodeIndex;
                while (nodeIndex != -1)
                {
                    yield return new KeyValuePair<TKey, TValue>(_nodes[nodeIndex]._key, _nodes[nodeIndex]._value);

                    nodeIndex = _nodes[nodeIndex]._nextNodeIndex;
                }
            }
        }
        #endregion
        #region ToArray
        /// <summary>
        /// Copies all the pairs in this dictionary to an array.
        /// This is useful if you plan to iterate many times as
        /// this copy will not change and because iterating in
        /// arrays is faster. But, if you plan to iterate only once,
        /// it is still faster to do it over the dictionary directly.
        /// </summary>
        public KeyValuePair<TKey, TValue>[] ToArray()
        {
            int count = Count;
            if (count == 0)
                return EmptyArray<KeyValuePair<TKey, TValue>>.Instance;

            var result = new KeyValuePair<TKey, TValue>[count];
            int resultIndex = -1;
            foreach (int firstNodeIndex in _indexes)
            {
                int nodeIndex = firstNodeIndex;
                while (nodeIndex != -1)
                {
                    resultIndex++;
                    result[resultIndex] = new KeyValuePair<TKey, TValue>(_nodes[nodeIndex]._key, _nodes[nodeIndex]._value);

                    nodeIndex = _nodes[nodeIndex]._nextNodeIndex;
                }
            }

            return result;
        }
        #endregion
        #region CopyKeys
        /// <summary>
        /// Copies all the keys in this dictionary to an array.
        /// This is useful if you plan to iterate many times as
        /// this copy will not change and because iterating in
        /// arrays is faster. But, if you plan to iterate only once,
        /// it is still faster to do it over the Keys directly.
        /// </summary>
        public TKey[] CopyKeys()
        {
            int count = Count;
            if (count == 0)
                return EmptyArray<TKey>.Instance;

            var result = new TKey[count];
            int resultIndex = -1;
            foreach (int firstNodeIndex in _indexes)
            {
                int nodeIndex = firstNodeIndex;
                while (nodeIndex != -1)
                {
                    resultIndex++;
                    result[resultIndex] = _nodes[nodeIndex]._key;

                    nodeIndex = _nodes[nodeIndex]._nextNodeIndex;
                }
            }

            return result;
        }
        #endregion
        #region CopyValues
        /// <summary>
        /// Copies all the values in this dictionary to an array.
        /// This is useful if you plan to iterate many times as
        /// this copy will not change and because iterating in
        /// arrays is faster. But, if you plan to iterate only once,
        /// it is still faster to do it over the Values directly.
        /// </summary>
        public TValue[] CopyValues()
        {
            int count = Count;
            if (count == 0)
                return EmptyArray<TValue>.Instance;

            var result = new TValue[count];
            int resultIndex = -1;
            foreach (int firstNodeIndex in _indexes)
            {
                int nodeIndex = firstNodeIndex;
                while (nodeIndex != -1)
                {
                    resultIndex++;
                    result[resultIndex] = _nodes[nodeIndex]._value;

                    nodeIndex = _nodes[nodeIndex]._nextNodeIndex;
                }
            }

            return result;
        }
        #endregion

        #region GetOrAdd
        public TValue GetOrAdd(TKey key, TValue value)
        {
            TValue oldValue;
            if (TryGetOrAdd(key, value, out oldValue))
                return oldValue;

            return value;
        }
        #endregion
        #region ForEach
        public void ForEach(ForEachDictionaryDelegate<TKey, TValue> action)
        {
            if (action == null)
                throw new ArgumentNullException("action");

            foreach (var pair in this)
                action(pair.Key, pair.Value);
        }
        #endregion
        #region TryAddMany
        public void TryAddMany(IEnumerable<KeyValuePair<TKey, TValue>> pairs)
        {
            if (pairs == null)
                throw new ArgumentNullException("pairs");

            foreach (var pair in pairs)
                TryAdd(pair.Key, pair.Value);
        }
        #endregion
        #region RemoveMany
        public void RemoveMany(IEnumerable<TKey> keys)
        {
            if (keys == null)
                throw new ArgumentNullException("keys");

            foreach (TKey key in keys)
                Remove(key);
        }
        #endregion
        #endregion

        #region IEnumerable Members
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        #endregion
        #region IExtendedDictionary Members
        Type IHashMap.TypeOfKeys
        {
            get { return typeof (TKey); }
        }

        Type IHashMap.TypeOfValues
        {
            get { return typeof (TValue); }
        }

        object IHashMap.this[object key]
        {
            get { return this[(TKey) key]; }
            set { this[(TKey) key] = (TValue) value; }
        }

        object IHashMap.Comparer
        {
            get { return Comparer; }
        }

        IEnumerable IHashMap.Keys
        {
            get { return Keys; }
        }

        IEnumerable IHashMap.Values
        {
            get { return Values; }
        }

        bool IHashMap.ContainsKey(object key)
        {
            return ContainsKey((TKey)key);
        }

        bool IHashMap.TryGetValue(object key, out object value)
        {
            TValue typedValue;
            bool result = TryGetValue((TKey)key, out typedValue);
            value = typedValue;
            return result;
        }

        bool IHashMap.TryGetOrAdd(object key, object value, out object oldValue)
        {
            TValue typedOldValue;
            bool result = TryGetOrAdd((TKey)key, (TValue)value, out typedOldValue);
            oldValue = typedOldValue;
            return result;
        }

        bool IHashMap.TryGetAndRemove(object key, out object value)
        {
            TValue typedValue;
            bool result = TryGetAndRemove((TKey)key, out typedValue);
            value = typedValue;
            return result;
        }

        bool IHashMap.TryGetAndAddOrReplace(object key, object value, out object oldValue)
        {
            TValue typedOldValue;
            bool result = TryGetAndAddOrReplace((TKey)key, (TValue)value, out typedOldValue);
            oldValue = typedOldValue;
            return result;
        }

        object IHashMap.GetValueOrDefault(object key)
        {
            return GetValueOrDefault((TKey)key);
        }

        object IHashMap.GetOrCreate(object key, Delegate createValue)
        {
            return GetOrCreate((TKey)key, (MyFunc<TKey, TValue>)createValue);
        }

        object IHashMap.GetOrAdd(object key, object value)
        {
            return GetOrAdd((TKey)key, (TValue)value);
        }

        bool IHashMap.Remove(object key)
        {
            return Remove((TKey)key);
        }

        object IHashMap.GetAndRemove(object key)
        {
            return GetAndRemove((TKey)key);
        }

        bool IHashMap.TryAdd(object key, object value)
        {
            return TryAdd((TKey)key, (TValue)value);
        }

        void IHashMap.Add(object key, object value)
        {
            Add((TKey)key, (TValue)value);
        }

        void IHashMap.ForEach(Delegate action)
        {
            ForEach((ForEachDictionaryDelegate<TKey, TValue>)action);
        }

        void IHashMap.AddMany(ICollection pairs)
        {
            AddMany((ICollection<KeyValuePair<TKey, TValue>>)pairs);
        }

        void IHashMap.AddOrReplaceMany(IEnumerable pairs)
        {
            AddOrReplaceMany((IEnumerable<KeyValuePair<TKey, TValue>>)pairs);
        }

        void IHashMap.TryAddMany(IEnumerable pairs)
        {
            TryAddMany((IEnumerable<KeyValuePair<TKey, TValue>>)pairs);
        }

        void IHashMap.RemoveMany(IEnumerable keys)
        {
            RemoveMany((IEnumerable<TKey>)keys);
        }

        void IHashMap.RemoveMany(Delegate checkDelegate)
        {
            RemoveMany((RemoveManyDictionaryDelegate<TKey, TValue>)checkDelegate);
        }

        Array IHashMap.ToArray()
        {
            return ToArray();
        }

        Array IHashMap.CopyKeys()
        {
            return CopyKeys();
        }

        Array IHashMap.CopyValues()
        {
            return CopyValues();
        }
        #endregion
    }
}

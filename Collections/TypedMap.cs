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

#if ENCRYPT
namespace JointCode.Internals
#else
using JointCode.Common.Extensions;
using JointCode.Common.Helpers;
using JointCode.Common.Threading;

namespace JointCode.Common.Collections
#endif
{
    public interface ITypedItem
    {
        /// <summary>
        /// The type that will be used as a key to retrieve this instance from the <seealso cref="TypedMap"/>.
        /// </summary>
        Type KeyType { get; }
        /// <summary>
        /// Tries to add this instance to the given <paramref name="typedMap"/>.
        /// </summary>
        /// <param name="typedMap">The <see cref="TypedMap"/> to add this instance to.</param>
        /// <returns></returns>
        bool TryAddTo(TypedMap typedMap);
        /// <summary>
        /// Removes this instance from the given <paramref name="typedMap"/>.
        /// </summary>
        /// <param name="typedMap">The <see cref="TypedMap"/> to remove this instance from.</param>
        /// <returns></returns>
        bool TryRemoveFrom(TypedMap typedMap);
    }

    public interface ITypedItem<TKey, TBase> : ITypedItem
        where TKey : class, ITypedItem<TKey, TBase>, TBase
        where TBase : class, ITypedItem
    { }

    /// <summary>
    /// A <see cref="TypedMap"/> is a fast, thread safe map between a type and an value of that type. 
    /// </summary>
    /// <example>
    /// The following code shows the usage of this class.
    /// <code>
    /// class InstanceCreatorTypedMap
    /// {
    ///     readonly UnsafeTypedMap _typedMap;
    ///     internal InstanceCreatorTypedMap()
    ///     {
    ///         _typedMap = UnsafeTypedMap.Create();
    ///     }
    ///     public bool TryAdd{TContract}(InstanceCreator{TContract} value)
    ///     {
    ///         return _typedMap.TryAdd{InstanceCreator{TContract}}(value);
    ///     }
    ///     public bool Remove{TContract}(InstanceCreator{TContract} value)
    ///     {
    ///         return _typedMap.Remove{InstanceCreator{TContract}}(value);
    ///     }
    ///     public InstanceCreator{TContract} GetValue{TContract}()
    ///     {
    ///         return _typedMap.GetValue{InstanceCreator{TContract}}();
    ///     }
    /// }
    /// public class InstanceCreator{TContract} : TypedItem{InstanceCreator{TContract}}
    /// {
    ///     readonly Type _conreteType;
    ///     public InstanceCreator(Type conreteType)
    ///     {
    ///         if (!Key.IsAssignableFrom(conreteType))
    ///             throw new ArgumentException("Invalid Type!");
    ///         _conreteType = conreteType;
    ///     }
    ///     public Type Key
    ///     {
    ///         get { return typeof(TContract); }
    ///     }
    ///     public TContract Create()
    ///     {
    ///         return (TContract)Activator.CreateInstance(_conreteType);
    ///     }
    /// }
    /// </code>
    /// </example>
    /// <remarks>
    /// It use a tricky that CLR does not share static fields between different generic types and it does not erase types.
    /// The internal data structure uses static fields to store values.
    /// </remarks>
    public abstract partial class TypedMap : CriticalDisposable
    {
        /// <summary>
        /// Gets the number of items registered in the map.
        /// </summary>
        public abstract int Count { get; }
        /// <summary>
        /// Clears all items registered in the map.
        /// </summary>
        public abstract void Clear();
        /// <summary>
        /// Determines whether the map contains an item registered with the specified type.
        /// </summary>
        /// <param name="keyType">The type which will be used as a key to retrieve corresponding item from the map.</param>
        public abstract bool ContainsKey(Type keyType);
        /// <summary>
        /// Releases resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources;
        /// <c>false</c> to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            Clear();
        }
        //public abstract void Trim();
    }

    partial class TypedMap
    {
        class RandomTypeProvider
        {
            int _typeIndex1;
            int _typeIndex21;
            int _typeIndex22;
            readonly object _syncRoot = new object();
            // concrete reference types
            static readonly List<Type> _concreteRefTypes;

            static RandomTypeProvider()
            {
                _concreteRefTypes = new List<Type>();
                var types = typeof(object).Assembly.GetTypes();
                foreach (var type in types)
                {
                    // skip the open generic types and value types, because:
                    // 1. an open generic type can not be generic parameter for other open generic types
                    // 2. if we use value types as generic parameter, this will make the CLR to create unique 
                    //    type for each one of them at runtime, thus unable to share code
                    if (type.IsValueType || type.ContainsGenericParameters || type.IsGenericTypeDefinition
                        || type.IsPointer || type.IsCOMObject || type.IsImport || type.IsSpecialName || type.Name.StartsWith("_"))
                        continue;
                    _concreteRefTypes.Add(type);
                }
            }

            internal void GetNextGenericArgumentType(out Type type1)
            {
                type1 = _concreteRefTypes[GetTypeIndexSafely()];
            }
            int GetTypeIndexSafely()
            {
                lock (_syncRoot)
                {
                    if (_typeIndex1 < _concreteRefTypes.Count)
                        return _typeIndex1++;
                    _typeIndex1 = 0;
                    return 0;
                }
            }

            internal void GetNextGenericArgumentTypes(out Type type1, out Type type2)
            {
                int index1, index2;
                GetTypeIndexSafely(out index1, out index2);
                type1 = _concreteRefTypes[index1];
                type2 = _concreteRefTypes[index2];
            }
            void GetTypeIndexSafely(out int index1, out int index2)
            {
                lock (_syncRoot)
                {
                    if (_typeIndex21 < _concreteRefTypes.Count)
                    {
                        index1 = _typeIndex21++;
                    }
                    else
                    {
                        _typeIndex21 = 0;
                        index1 = 0;
                    }
                    if (_typeIndex22 < _concreteRefTypes.Count)
                    {
                        index2 = _typeIndex22++;
                    }
                    else
                    {
                        _typeIndex22 = 0;
                        index2 = 0;
                    }
                }
            }
        }

        static readonly RandomTypeProvider _typeProvider = new RandomTypeProvider();

        /// <summary>
        /// Creates an instance of <see cref="TypedMap"/>.
        /// </summary>
        /// <returns></returns>
        public static TypedMap<TBase> Create<TBase>()
            where TBase : class, ITypedItem
        {
            Type type1, type2;
            _typeProvider.GetNextGenericArgumentTypes(out type1, out type2);

            IReaderWriterLockSlim lockSlim;
            if (SystemHelper.HasMultiProcessors)
                lockSlim = new OptimisticReaderWriterLock();
            else
                lockSlim = new SpinReaderWriterLockSlim();

            var closedMapType = typeof(InnerTypedMap<,,,>).MakeGenericType(typeof(TBase), lockSlim.GetType(), type1, type2);
            return Activator.CreateInstance(closedMapType, new object[] { lockSlim }) as TypedMap<TBase>;
        }

        public static bool TryAddTo<TKey, TBase>(ITypedItem<TKey, TBase> typedItem, TypedMap typedMap)
            where TKey : class, ITypedItem<TKey, TBase>, TBase
            where TBase : class, ITypedItem
        {
            Requires.Instance.NotNull(typedMap, "typedMap");
            Requires.Instance.NotNull(typedItem, "typedItem");

            var map = typedMap as TypedMap<TBase>;
            if (map == null)
                throw new InvalidOperationException("");
            var item = typedItem as TKey;
            if (item == null)
                throw new InvalidOperationException("");
            return map.TryAddUnsynchronized(item);
        }

        public static bool TryRemoveFrom<TKey, TBase>(ITypedItem<TKey, TBase> typedItem, TypedMap typedMap)
            where TKey : class, ITypedItem<TKey, TBase>, TBase
            where TBase : class, ITypedItem
        {
            Requires.Instance.NotNull(typedMap, "typedMap");
            Requires.Instance.NotNull(typedItem, "typedItem");

            var map = typedMap as TypedMap<TBase>;
            if (map == null)
                throw new InvalidOperationException("");
            var item = typedItem as TKey;
            if (item == null)
                throw new InvalidOperationException("");
            return map.TryRemoveUnsynchronized(item);
        }
    }

    public abstract class TypedMap<TBase> : TypedMap, IEnumerable<KeyValuePair<Type, TBase>> 
        where TBase : class, ITypedItem
    {
        /// <summary>
        /// Gets the item registered into the map with type <typeparamref name="TKey"/>.
        /// </summary>
        /// <typeparam name="TKey">The type of the item.</typeparam>
        /// <returns>The item registered with specified <typeparamref name="TKey"/>, or <code>null</code> if no such item found.</returns>
        public abstract TKey GetValue<TKey>() where TKey : class, ITypedItem<TKey, TBase>, TBase;
        /// <summary>
        /// Tries to add an item to the map.
        /// </summary>
        /// <typeparam name="TKey">The type of the item.</typeparam>
        /// <param name="item">The item.</param>
        /// <returns>
        /// A boolean indicating whether the operation is successful. If it is not, then the user is responsible
        /// for releasing the unnecessary resource. See the code example above.
        /// </returns>
        /// <example>
        /// if (!TryAdd{ISomeType}(aDisposableInstance))
        /// {
        ///     aDisposableInstance.Dispose();
        /// }
        ///   </example>
        public abstract bool TryAdd<TKey>(TKey item) where TKey : class, ITypedItem<TKey, TBase>, TBase;
        /// <summary>
        /// Tries to remove the item registered with the type <typeparamref name="TKey"/> from the map.
        /// </summary>
        /// <typeparam name="TKey">The type of the item.</typeparam>
        /// <param name="item">The item.</param>
        /// <returns></returns>
        public abstract bool TryRemove<TKey>(TKey item) where TKey : class, ITypedItem<TKey, TBase>, TBase;

        /// <summary>
        /// Tries to get an existing item from the map, or add a new item to the map and return it, if one does not exist.
        /// </summary>
        /// <typeparam name="TKey">The type of the derived.</typeparam>
        /// <param name="item">The item.</param>
        /// <param name="retVal">The return value.</param>
        /// <returns><code>true</code> if the item is successfully added, or <code>false</code> if not.</returns>
        public abstract bool TryGetOrAdd<TKey>(TKey item, out TKey retVal) where TKey : class, ITypedItem<TKey, TBase>, TBase;
        // oldItem: if it is an update operation, and if the oldItem implements IDisposable, then it must be disposed.
        public abstract bool TryAddOrUpdate<TKey>(TKey newItem, out TKey oldItem) where TKey : class, ITypedItem<TKey, TBase>, TBase;
        public abstract bool TryGetOrAdd<TKey>(MyFunc<TKey> itemFactory, out TKey retVal) where TKey : class, ITypedItem<TKey, TBase>, TBase;
        // oldItem: if it is an update operation, and if the oldItem implements IDisposable, then it must be disposed.
        public abstract bool TryAddOrUpdate<TKey>(MyFunc<TKey> newItemFactory, out TKey newItem, out TKey oldItem) where TKey : class, ITypedItem<TKey, TBase>, TBase;

        /// <summary>
        /// Gets the item registered into the map with given type.
        /// </summary>
        /// <param name="keyType">The type which will be used as a key to retrieve corresponding item from the map.</param>
        /// <returns>The item registered with specified <paramref name="keyType"/>, or <code>null</code> if no such item found.</returns>
        public abstract TBase GetValue(Type keyType);
        public abstract bool TryAdd(TBase item);
        public abstract bool TryRemove(TBase item);

        public abstract bool TryAddMany(IEnumerable<TBase> items);
        public abstract bool TryRemoveMany(IEnumerable<TBase> items);

        public abstract bool TryGetOrAdd(TBase item, out TBase retVal);
        public abstract bool TryAddOrUpdate(TBase newItem, out TBase oldItem);
        //public abstract bool TryGetOrAdd(Func<TBase> itemFactory, out TBase retVal);
        //public abstract bool TryAddOrUpdate(Func<TBase> newItemFactory, out TBase newItem, out TBase oldItem);

        /// <summary>
        /// Adds the item to the map with the type <typeparamref name="TKey"/> unsynchronized.
        /// </summary>
        /// <typeparam name="TKey">The type of the item.</typeparam>
        /// <param name="item">The item.</param>
        /// <returns></returns>
        internal abstract bool TryAddUnsynchronized<TKey>(TKey item) where TKey : class, ITypedItem<TKey, TBase>, TBase;
        /// <summary>
        /// Removes the item registered with the type <typeparamref name="TKey"/> from the map unsynchronized.
        /// </summary>
        /// <typeparam name="TKey">The type of the item.</typeparam>
        /// <param name="item">The item.</param>
        /// <returns></returns>
        internal abstract bool TryRemoveUnsynchronized<TKey>(TKey item) where TKey : class, ITypedItem<TKey, TBase>, TBase;

        public abstract IEnumerator<KeyValuePair<Type, TBase>> GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    abstract class InnerTypedMap<TBase, TReaderWriterLockSlim> : TypedMap<TBase>
        where TBase : class, ITypedItem
        where TReaderWriterLockSlim : IReaderWriterLockSlim
    {
        protected readonly TReaderWriterLockSlim _lockSlim;
        Dictionary<Type, TBase> _stubs/* = new Dictionary<Type, TBase>()*/;

        protected InnerTypedMap(TReaderWriterLockSlim lockSlim)
        {
            _lockSlim = lockSlim;
        }

        public override int Count
        {
            get { return _stubs == null ? 0 : _stubs.Count; }
        }

        public override bool TryGetOrAdd(TBase item, out TBase retVal)
        {
            Requires.Instance.NotNull(item, "item");
            AssertNotDisposed();

            _lockSlim.EnterWriteLock();
            try
            {
                if (_stubs != null && _stubs.Count > 0 && _stubs.TryGetValue(item.KeyType, out retVal))
                    return false;

                item.TryAddTo(this);
                retVal = item;
                return true;
            }
            finally
            {
                _lockSlim.ExitWriteLock();
            }
        }

        public override bool TryAddOrUpdate(TBase newItem, out TBase oldItem)
        {
            Requires.Instance.NotNull(newItem, "newItem");
            _lockSlim.EnterWriteLock();
            AssertNotDisposed();

            try
            {
                if (_stubs != null && _stubs.Count > 0 && _stubs.TryGetValue(newItem.KeyType, out oldItem))
                {
                    // Low efficience. 
                    // Needs a [bool Dictionary<TKey, TValue>.TryRemove(Type keyType, out TValue value)] 
                    // or [bool Dictionary<TKey, TValue>.TryAddOrUpdate(Type keyType, out TValue value)] method to improve performance.
                    oldItem.TryRemoveFrom(this);
                    //_stubs.Remove(oldItem.KeyType); 
                    newItem.TryAddTo(this);
                    return false;
                }
                else
                {
                    newItem.TryAddTo(this);
                    oldItem = null;
                    return true;
                }
            }
            finally
            {
                _lockSlim.ExitWriteLock();
            }
        }

        //public override bool TryGetOrAdd(Func<TBase> itemFactory, out TBase retVal)
        //{
        //    Requires.Instance.NotNull(keyType, "keyType");
        //    Requires.Instance.NotNull(itemFactory, "itemFactory");

        //    _lockSlim.EnterWriteLock();
        //    try
        //    {
        //        if (_stubs != null && _stubs.Count > 0 && _stubs.TryGetValue(keyType, out retVal))
        //            return false;

        //        _stubs = _stubs ?? new Dictionary<Type, TBase>();
        //        var item = itemFactory();
        //        item.TryAddTo(this, false);
        //        retVal = item;
        //        return true;
        //    }
        //    finally
        //    {
        //        _lockSlim.ExitWriteLock();
        //    }
        //}

        //public override bool TryAddOrUpdate(Func<TBase> newItemFactory, out TBase newItem, out TBase oldItem)
        //{
        //    Requires.Instance.NotNull(keyType, "keyType");
        //    Requires.Instance.NotNull(newItemFactory, "itemFactory");

        //    newItem = newItemFactory();

        //    _lockSlim.EnterWriteLock();
        //    try
        //    {
        //        if (_stubs != null && _stubs.Count > 0 && _stubs.TryGetValue(keyType, out oldItem))
        //        {

        //            return false;
        //        }

        //        _stubs = _stubs ?? new Dictionary<Type, TBase>();
        //        newItem.TryAddTo(this, false);
        //        oldItem = null;
        //        return true;
        //    }
        //    finally
        //    {
        //        _lockSlim.ExitWriteLock();
        //    }
        //}

        public override bool TryAdd(TBase item)
        {
            Requires.Instance.NotNull(item, "item");
            AssertNotDisposed();

            _lockSlim.EnterWriteLock();
            try
            {
                return item.TryAddTo(this);
            }
            finally
            {
                _lockSlim.ExitWriteLock();
            }
        }

        public override bool TryRemove(TBase item)
        {
            Requires.Instance.NotNull(item, "item");
            AssertNotDisposed();

            _lockSlim.EnterWriteLock();
            try
            {
                return item.TryRemoveFrom(this);
            }
            finally
            {
                _lockSlim.ExitWriteLock();
            }
        }

        public override bool TryAddMany(IEnumerable<TBase> items)
        {
            Requires.Instance.NotNull(items, "items");
            AssertNotDisposed();

            _lockSlim.EnterWriteLock();
            try
            {
                var result = true;
                foreach (var item in items)
                    result = item.TryAddTo(this) && result;
                return result;
            }
            finally
            {
                _lockSlim.ExitWriteLock();
            }
        }

        public override bool TryRemoveMany(IEnumerable<TBase> items)
        {
            Requires.Instance.NotNull(items, "items");
            AssertNotDisposed();

            if (_stubs == null)
                return false;

            _lockSlim.EnterWriteLock();
            try
            {
                if (_stubs == null)
                    return false;
                var result = true;
                foreach (var item in items)
                    result = item.TryRemoveFrom(this) && result;
                return result;
            }
            finally
            {
                _lockSlim.ExitWriteLock();
            }
        }

        public override void Clear()
        {
            AssertNotDisposed();
            // make sure we are not disposing multiple times.
            var stubs = _stubs;
            if (stubs == null)
                return;

            _lockSlim.EnterWriteLock();
            try
            {
                // collect all items store in the stub.
                var pairs = new List<KeyValuePair<Type, TBase>>(stubs);
                for (int i = pairs.Count - 1; i >= 0; i--)
                    pairs[i].Value.TryRemoveFrom(this);
                _stubs = null;
            }
            finally
            {
                _lockSlim.ExitWriteLock();
            }
        }

        public override bool ContainsKey(Type keyType)
        {
            Requires.Instance.NotNull(keyType, "keyType");
            AssertNotDisposed();

            _lockSlim.EnterReadLock();
            try
            {
                return (_stubs == null || _stubs.Count == 0) ? false : _stubs.ContainsKey(keyType);
            }
            finally
            {
                _lockSlim.ExitReadLock();
            }
        }

        public override TBase GetValue(Type keyType)
        {
            Requires.Instance.NotNull(keyType, "keyType");
            AssertNotDisposed();

            _lockSlim.EnterReadLock();
            try
            {
                TBase item;
                return _stubs == null
                    ? null
                    : (_stubs.TryGetValue(keyType, out item) ? item : null);
            }
            finally
            {
                _lockSlim.ExitReadLock();
            }
        }

        public override IEnumerator<KeyValuePair<Type, TBase>> GetEnumerator()
        {
            AssertNotDisposed();
            if (_stubs == null)
                return null;

            List<KeyValuePair<Type, TBase>> stubs = null;
            _lockSlim.EnterReadLock();
            try
            {
                if (_stubs == null)
                    return null;
                stubs = new List<KeyValuePair<Type, TBase>>(_stubs);
            }
            finally
            {
                _lockSlim.ExitReadLock();
            }

            return stubs.GetEnumerator();
        }

        protected void UpdateItemInDict(TBase item)
        {
            _stubs = _stubs ?? new Dictionary<Type, TBase>();
            _stubs[item.KeyType] = item;
        }

        protected void AddItemToDict(TBase item)
        {
            _stubs = _stubs ?? new Dictionary<Type, TBase>();
            // A same dictionary key might exist although the [ItemHolder<TKey>.Item == null] is true, 
            // so we still needs to use a try...catch clause here!
            try
            {
                _stubs.Add(item.KeyType, item);
            }
            catch (ArgumentException)
            {
                throw new ArgumentException("An item of same key type (ITypedItem.KeyType) already exists in the TypedMap! This might happen because the internal logic of typed items is invalid! There are two different typed items which has same key type!");
            }
        }

        protected void RemoveItemFromDict(Type keyType)
        {
            if (_stubs == null || !_stubs.Remove(keyType))
                throw new InvalidOperationException("The internal state for TypedMap is invalid!");
        }

        //void VerifyStubsNotNull()
        //{
        //    if (_stubs == null)
        //        throw new InvalidOperationException("Can not remove item from the map before adding any items to it!");
        //}
    }

    /// Two extra generic parameters (<typeparamref name="TDistrinct1"/> and <typeparamref name="TDistrinct2"/>) are introduced on purpose, 
    /// so that we can build a new different closed generic subclass of <see cref="TypedMap"/> by only changing the <typeparamref name="TDistrinct1"/> 
    /// and <typeparamref name="TDistrinct2"/>.
    class InnerTypedMap<TBase, TReaderWriterLockSlim, TDistrinct1, TDistrinct2> : InnerTypedMap<TBase, TReaderWriterLockSlim>
        where TBase : class, ITypedItem
        where TReaderWriterLockSlim : IReaderWriterLockSlim
    {
        /// By this class, we use a tricky that:
        /// 1. CLR does not share static fields between different generic types
        /// 2. CLR does not erase types ever.
        static class ItemHolder<TKey> where TKey : class
        {
            internal static TKey Item;
        }

        public InnerTypedMap(TReaderWriterLockSlim lockSlim)
            : base(lockSlim) { }

        public override bool TryAdd<TKey>(TKey item)
        {
            Requires.Instance.NotNull(item, "item");
            AssertNotDisposed();

            if (ItemHolder<TKey>.Item != null)
                return false;

            _lockSlim.EnterWriteLock();
            try
            {
                if (ItemHolder<TKey>.Item != null)
                    return false;
                AddItemToDict(item);
                ItemHolder<TKey>.Item = item;
                return true;
            }
            finally
            {
                _lockSlim.ExitWriteLock();
            }
        }

        public override bool TryRemove<TKey>(TKey item)
        {
            Requires.Instance.NotNull(item, "item");
            AssertNotDisposed();

            if (ItemHolder<TKey>.Item != item)
                return false;

            _lockSlim.EnterWriteLock();
            try
            {
                if (ItemHolder<TKey>.Item != item) // item removed by another thread already, nearly at the same time
                    return false;
                RemoveItemFromDict(item.KeyType);
                ItemHolder<TKey>.Item = null;
                return true;
            }
            finally
            {
                _lockSlim.ExitWriteLock();
            }
        }

        public override TKey GetValue<TKey>()
        {
            return ItemHolder<TKey>.Item;
        }

        public override bool TryGetOrAdd<TKey>(TKey item, out TKey retVal)
        {
            Requires.Instance.NotNull(item, "item");
            AssertNotDisposed();

            retVal = ItemHolder<TKey>.Item;
            if (retVal != null) return false;

            _lockSlim.EnterWriteLock();
            try
            {
                retVal = ItemHolder<TKey>.Item;
                if (retVal != null) return false;

                AddItemToDict(item);
                ItemHolder<TKey>.Item = item;
                retVal = item;
                return true;
            }
            finally
            {
                _lockSlim.ExitWriteLock();
            }
        }

        public override bool TryAddOrUpdate<TKey>(TKey newItem, out TKey oldItem)
        {
            Requires.Instance.NotNull(newItem, "newItem");
            AssertNotDisposed();

            _lockSlim.EnterWriteLock();
            try
            {
                oldItem = ItemHolder<TKey>.Item;
                if (oldItem == null)
                {
                    AddItemToDict(newItem);
                    ItemHolder<TKey>.Item = newItem;
                    return true;
                }
                else
                {
                    UpdateItemInDict(newItem);
                    ItemHolder<TKey>.Item = newItem;
                    return false;
                }
            }
            finally
            {
                _lockSlim.ExitWriteLock();
            }
        }

        public override bool TryGetOrAdd<TKey>(MyFunc<TKey> itemFactory, out TKey retVal)
        {
            Requires.Instance.NotNull(itemFactory, "itemFactory");
            AssertNotDisposed();

            retVal = ItemHolder<TKey>.Item;
            if (retVal != null) return false;

            _lockSlim.EnterWriteLock();
            try
            {
                retVal = ItemHolder<TKey>.Item;
                if (retVal != null) return false;

                var item = itemFactory();
                AddItemToDict(item);
                ItemHolder<TKey>.Item = item;
                retVal = item;
                return true;
            }
            finally
            {
                _lockSlim.ExitWriteLock();
            }
        }

        public override bool TryAddOrUpdate<TKey>(MyFunc<TKey> newItemFactory, out TKey newItem, out TKey oldItem)
        {
            Requires.Instance.NotNull(newItemFactory, "newItemFactory");
            AssertNotDisposed();

            newItem = newItemFactory();

            _lockSlim.EnterWriteLock();
            try
            {
                oldItem = ItemHolder<TKey>.Item;
                if (oldItem == null)
                {
                    AddItemToDict(newItem);
                    ItemHolder<TKey>.Item = newItem;
                    return true;
                }
                else
                {
                    UpdateItemInDict(newItem);
                    ItemHolder<TKey>.Item = newItem;
                    return false;
                }
            }
            finally
            {
                _lockSlim.ExitWriteLock();
            }
        }

        internal override bool TryAddUnsynchronized<TKey>(TKey item)
        {
            if (ItemHolder<TKey>.Item != null)
                return false;
            AddItemToDict(item);
            ItemHolder<TKey>.Item = item;
            return true;
        }

        internal override bool TryRemoveUnsynchronized<TKey>(TKey item)
        {
            if (ItemHolder<TKey>.Item != item)
                return false;
            RemoveItemFromDict(item.KeyType);
            ItemHolder<TKey>.Item = null;
            return true;
        }
    }

    #region Old Implementation
    ///// <summary>
    ///// Define a base type for the typed item that will be managed by the <see cref="TypedMap"/>.
    ///// </summary>
    //public abstract class TypedItem
    //{
    //    /// <summary>
    //    /// Initializes a new instance of the <see cref="TypedItem"/> class.
    //    /// </summary>
    //    /// <remarks>
    //    /// This constructor is marked as <c>internal</c>, which means that this type is 
    //    /// supposed to be derived from other types located in the same assembly only,
    //    /// types from other assemblies should not derive this type directly.
    //    /// </remarks>
    //    internal TypedItem() { }

    //    public abstract bool TryAddTo(TypedMap typedMap);
    //    public abstract bool RemoveFrom(TypedMap typedMap);
    //}

    ///// <summary>
    ///// Define a base type for the typed item that will be managed by the <see cref="TypedMap"/>.
    ///// An instance of <see cref="TypedItem{TMapKey}"/> is some kind of 'know itself' sense strongly
    ///// (compare to weak) object, in that the generic parameter <typeparamref name="TMapKey"/> points
    ///// out what type it actually is.
    ///// </summary>
    ///// <typeparam name="TMapKey">
    ///// The type of the derived class of this class, which will be used as the generic type key of 
    ///// <see cref="TypedMap"/> to add, remove and retrieve items.
    ///// </typeparam>
    ///// <example>
    ///// The following code shows the usage of this class.
    ///// <code>
    ///// class InstanceCreatorTypedMap
    ///// {
    /////     readonly UnsafeTypedMap _typedMap;
    /////     internal InstanceCreatorTypedMap()
    /////     {
    /////         _typedMap = UnsafeTypedMap.Create();
    /////     }
    /////     public void Add{TContract}(InstanceCreator{TContract} value)
    /////     {
    /////         _typedMap.TryAdd{InstanceCreator{TContract}}(value);
    /////     }
    /////     public bool Remove{TContract}(InstanceCreator{TContract} value)
    /////     {
    /////         return _typedMap.Remove{InstanceCreator{TContract}}(value);
    /////     }
    /////     public InstanceCreator{TContract} GetValue{TContract}()
    /////     {
    /////         return _typedMap.GetValue{InstanceCreator{TContract}}();
    /////     }
    ///// }
    ///// public class InstanceCreator{TContract} : TypedItem{InstanceCreator{TContract}}
    ///// {
    /////     readonly Type _conreteType;
    /////     public InstanceCreator(Type conreteType)
    /////     {
    /////         if (!Key.IsAssignableFrom(conreteType))
    /////             throw new ArgumentException("Invalid Type!");
    /////         _conreteType = conreteType;
    /////     }
    /////     public Type Key
    /////     {
    /////         get { return typeof(TContract); }
    /////     }
    /////     public TContract Create()
    /////     {
    /////         return (TContract)Activator.CreateInstance(_conreteType);
    /////     }
    ///// }
    ///// </code>
    ///// </example>
    //public abstract class TypedItem<TMapKey> : TypedItem
    //    where TMapKey : TypedItem<TMapKey>
    //{
    //    public sealed override bool TryAddTo(TypedMap typedMap)
    //    {
    //        var derived = this as TMapKey;
    //        return typedMap.TryAdd(derived);
    //    }

    //    public sealed override bool RemoveFrom(TypedMap typedMap)
    //    {
    //        var derived = this as TMapKey;
    //        return typedMap.Remove(derived);
    //    }
    //}

    //partial class TypedMap
    //{
    //    /// <summary>
    //    /// Internal implementation.
    //    /// </summary>
    //    abstract class InnerTypedMap<TReaderWriterLockSlim> : TypedMap
    //        where TReaderWriterLockSlim : IReaderWriterLockSlim
    //    {
    //        readonly TReaderWriterLockSlim _lockSlim;
    //        Dictionary<Type, TypedItem> _stubs = new Dictionary<Type, TypedItem>();

    //        protected InnerTypedMap(TReaderWriterLockSlim lockSlim)
    //        {
    //            _lockSlim = lockSlim;
    //        }

    //        protected void AddItem(Type itemType, TypedItem item)
    //        {
    //            _lockSlim.EnterWriteLock();
    //            try
    //            {
    //                _stubs.Add(itemType, item);
    //            }
    //            finally
    //            {
    //                _lockSlim.ExitWriteLock();
    //            }
    //        }

    //        protected void RemoveItem(Type itemType)
    //        {
    //            _lockSlim.EnterWriteLock();
    //            try
    //            {
    //                _stubs.Remove(itemType);
    //            }
    //            finally
    //            {
    //                _lockSlim.ExitWriteLock();
    //            }
    //        }

    //        public override int Count
    //        {
    //            get { return _stubs.Count; }
    //        }

    //        public override void Clear()
    //        {
    //            // make sure we are not disposing multiple times.
    //            var stubs = _stubs;
    //            if (stubs == null || Interlocked.CompareExchange(ref _stubs, null, stubs) == null)
    //                return;

    //            _lockSlim.EnterWriteLock();
    //            try
    //            {
    //                // collect all items store in the stub.
    //                var typedItems = stubs.Values;
    //                foreach (var typedItem in typedItems)
    //                    typedItem.RemoveFrom(this);
    //            }
    //            finally
    //            {
    //                _lockSlim.ExitWriteLock();
    //            }
    //        }

    //        public override bool ContainsKey(Type type)
    //        {
    //            _lockSlim.EnterReadLock();
    //            try
    //            {
    //                return _stubs.ContainsKey(type);
    //            }
    //            finally
    //            {
    //                _lockSlim.ExitReadLock();
    //            }
    //        }

    //        public override TypedItem GetValue(Type type)
    //        {
    //            _lockSlim.EnterReadLock();
    //            try
    //            {
    //                TypedItem item;
    //                return _stubs.TryGetValue(type, out item) ? item : null;
    //            }
    //            finally
    //            {
    //                _lockSlim.ExitReadLock();
    //            }
    //        }
    //    }

    //    /// <summary>
    //    /// Internal implementation.
    //    /// </summary>
    //    /// <remarks>
    //    /// Two extra generic parameters (<typeparamref name="TDistrinct1"/> and <typeparamref name="TDistrinct2"/>) are introduced on purpose, 
    //    /// so that we can build a new different closed generic subclass of <see cref="TypedMap"/> by only changing the <typeparamref name="TDistrinct1"/> and <typeparamref name="TDistrinct2"/>.
    //    /// </remarks>
    //    class InnerTypedMap<TReaderWriterLockSlim, TDistrinct1, TDistrinct2> : InnerTypedMap<TReaderWriterLockSlim>
    //        where TReaderWriterLockSlim : IReaderWriterLockSlim
    //    {
    //        /// By this class, we use a tricky that:
    //        /// 1. CLR does not share static fields between different generic types
    //        /// 2. CLR does not erase types ever.
    //        static class ItemHolder<TItem> where TItem : class
    //        {
    //            internal static TItem Item;
    //        }

    //        public InnerTypedMap(TReaderWriterLockSlim lockSlim)
    //            : base(lockSlim)
    //        {
    //        }

    //        public override bool TryAdd<TItem>(TItem item)
    //        {
    //            if (Interlocked.CompareExchange(ref ItemHolder<TItem>.Item, item, null) != null)
    //                return false;
    //            AddItem(typeof(TItem), item);
    //            return true;
    //        }

    //        public override bool Remove<TItem>(TItem item)
    //        {
    //            if (Interlocked.CompareExchange(ref ItemHolder<TItem>.Item, null, item) != item)
    //                return false;
    //            RemoveItem(typeof(TItem));
    //            return true;
    //        }

    //        public override TItem GetValue<TItem>()
    //        {
    //            return ItemHolder<TItem>.Item;
    //        }
    //    }
    //}

    ///// <summary>
    ///// A <see cref="TypedMap"/> is a fast, thread safe map between a type and an value of that type. 
    ///// </summary>
    ///// <example>
    ///// The following code shows the usage of this class.
    ///// <code>
    ///// class InstanceCreatorTypedMap
    ///// {
    /////     readonly UnsafeTypedMap _typedMap;
    /////     internal InstanceCreatorTypedMap()
    /////     {
    /////         _typedMap = UnsafeTypedMap.Create();
    /////     }
    /////     public bool TryAdd{TContract}(InstanceCreator{TContract} value)
    /////     {
    /////         return _typedMap.TryAdd{InstanceCreator{TContract}}(value);
    /////     }
    /////     public bool Remove{TContract}(InstanceCreator{TContract} value)
    /////     {
    /////         return _typedMap.Remove{InstanceCreator{TContract}}(value);
    /////     }
    /////     public InstanceCreator{TContract} GetValue{TContract}()
    /////     {
    /////         return _typedMap.GetValue{InstanceCreator{TContract}}();
    /////     }
    ///// }
    ///// public class InstanceCreator{TContract} : TypedItem{InstanceCreator{TContract}}
    ///// {
    /////     readonly Type _conreteType;
    /////     public InstanceCreator(Type conreteType)
    /////     {
    /////         if (!Key.IsAssignableFrom(conreteType))
    /////             throw new ArgumentException("Invalid Type!");
    /////         _conreteType = conreteType;
    /////     }
    /////     public Type Key
    /////     {
    /////         get { return typeof(TContract); }
    /////     }
    /////     public TContract Create()
    /////     {
    /////         return (TContract)Activator.CreateInstance(_conreteType);
    /////     }
    ///// }
    ///// </code>
    ///// </example>
    ///// <remarks>
    ///// It use a tricky that CLR does not share static fields between different generic types and it does not erase types.
    ///// The internal data structure uses static fields to store values.
    ///// </remarks>
    //public abstract partial class TypedMap : CriticalDisposable
    //{
    //    static readonly RandomTypeProvider _typeProvider = new RandomTypeProvider();

    //    /// <summary>
    //    /// Gets the number of items registered in the map.
    //    /// </summary>
    //    public abstract int Count { get; }
    //    /// <summary>
    //    /// Clears all items registered in the map.
    //    /// </summary>
    //    public abstract void Clear();
    //    /// <summary>
    //    /// Determines whether the map contains an item registered with the specified type.
    //    /// </summary>
    //    /// <param name="type">The type.</param>
    //    public abstract bool ContainsKey(Type type);
    //    /// <summary>
    //    /// Gets the item registered into the map with given type.
    //    /// </summary>
    //    /// <param name="type">The type.</param>
    //    /// <returns></returns>
    //    public abstract TypedItem GetValue(Type type);
    //    /// <summary>
    //    /// Tries to add an item to the map.
    //    /// </summary>
    //    /// <typeparam name="TItem">The type of the item.</typeparam>
    //    /// <param name="item">The item.</param>
    //    /// <example>
    //    /// if (!TryAdd{ISomeType}(aDisposableInstance))
    //    /// {
    //    ///     aDisposableInstance.Dispose();
    //    /// }
    //    /// </example>
    //    /// <returns>
    //    /// A boolean indicating whether the operation is successful. If it is not, then the user is responsible 
    //    /// for releasing the unnecessary resource. See the code example above.
    //    /// </returns>
    //    public abstract bool TryAdd<TItem>(TItem item) where TItem : TypedItem<TItem>;
    //    /// <summary>
    //    /// Gets the item registered into the map with type <typeparamref name="TItem"/>.
    //    /// </summary>
    //    /// <typeparam name="TItem">The type of the item.</typeparam>
    //    /// <returns>The registered item if one is found, or null.</returns>
    //    public abstract TItem GetValue<TItem>() where TItem : TypedItem<TItem>;
    //    /// <summary>
    //    /// Removes the item registered with the type <typeparamref name="TItem"/> from the map.
    //    /// </summary>
    //    /// <typeparam name="TItem">The type of the item.</typeparam>
    //    /// <param name="item">The item.</param>
    //    /// <returns></returns>
    //    public abstract bool Remove<TItem>(TItem item) where TItem : TypedItem<TItem>;

    //    /// <summary>
    //    /// Releases resources.
    //    /// </summary>
    //    /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources;
    //    /// <c>false</c> to release only unmanaged resources.</param>
    //    protected override void Dispose(bool disposing)
    //    {
    //        Clear();
    //    }

    //    /// <summary>
    //    /// Creates an instance of <see cref="TypedMap"/>.
    //    /// </summary>
    //    /// <returns></returns>
    //    public static TypedMap Create()
    //    {
    //        Type type1, type2;
    //        _typeProvider.GetNextGenericArgumentTypes(out type1, out type2);

    //        IReaderWriterLockSlim lockSlim;
    //        if (SystemHelper.HasMultiProcessors)
    //            lockSlim = new OptimisticReaderWriterLock();
    //        else
    //            lockSlim = new SpinReaderWriterLockSlim();

    //        var closedMapType = typeof(InnerTypedMap<,,>).MakeGenericType(lockSlim.GetType(), type1, type2);
    //        return Activator.CreateInstance(closedMapType, new object[] { lockSlim }) as TypedMap;
    //    }
    //} 
    #endregion
}
//using System;
//using System.Collections.Generic;
//using System.Threading;
//using JointCode.Common.Extensions;
//using JointCode.Common.Threading;

//namespace JointCode.Common.Collections
//{
////    partial class MultiTypedMap
////    {
////        protected abstract class AbstractItemStore<TItem> where TItem : class
////        {
////            internal abstract int Count { get; }
////            internal abstract TItem GetFirst();
////            internal abstract List<TItem> GetAll();
////            internal abstract bool Remove(TItem item);
////            internal abstract void Add(TItem item);

////            internal abstract List<TItem> GetAll(IInjectionTargetInfo targetInfo);
////            internal abstract bool TryGetFirst(IInjectionTargetInfo targetInfo, out TItem item);
////            internal abstract AbstractItemStore<TItem> DeepClone();
////        }
////        protected class SoleItemStore<TItem> : AbstractItemStore<TItem>
////            where TItem : class
////        {
////            TItem _item;

////            internal SoleItemStore(TItem item)
////            {
////                _item = item;
////            }

////            internal override int Count
////            {
////                get { return _item != null ? 1 : 0; }
////            }

////            internal override TItem GetFirst()
////            {
////                return _item;
////            }

////            internal override bool Remove(TItem item)
////            {
////                if (!ReferenceEquals(_item, item))
////                    return false;
////                _item = null;
////                return true;
////            }

////            internal override void Add(TItem item)
////            {
////                throw new NotImplementedException("This method should not be called!");
////            }

////            internal override AbstractItemStore<TItem> DeepClone()
////            {
////                return new SoleItemStore<TItem>(_item);
////            }
////        }
////        protected class PolyItemStore<TItem> : AbstractItemStore<TItem>
////            where TItem : class
////        {
////            readonly List<TItem> _items;

////            internal PolyItemStore(TItem item)
////            {
////                _items = new List<TItem> { item };
////            }
////            PolyItemStore(List<TItem> items)
////            {
////                _items = new List<TItem>(items);
////            }

////            internal override int Count
////            {
////                get { return _items.Count; }
////            }

////            internal override TItem GetFirst()
////            {
////                return _items[0];
////            }

////            internal override bool Remove(TItem item)
////            {
////                return _items.Remove(item);
////            }

////            internal override void Add(TItem item)
////            {
////                _items.Add(item);
////            }

////            internal override AbstractItemStore<TItem> DeepClone()
////            {
////                return new PolyItemStore<TItem>(_items);
////            }
////        }

////        // We introduce 2 generic parameters (TDistrinct1 and TDistrinct2) on purpose, so that we can build a new different 
////        // closed generic subclass of MultiTypedMap by only changing the TDistrinct1 and TDistrinct2.
////        // This type uses a technique named "hot swap" to ensure thread safety. See http://www.codeproject.com/Articles/92679/Hot-Swap-Thread-Safe-Collections
////        class InnerTypedMap<TDistrinct1, TDistrinct2> : MultiTypedMap
////        {
////            // Use a tricky that c# does not share static fields between different generic types.
////            static class ItemHolder<TItem> where TItem : TBaseTypedItem
////            {
////                // The ItemStore will never be changed, only swapped when a new one is generated.
////                internal static volatile AbstractItemStore<TItem> ItemStore;
////            }

////            public override void Add<TItem>(TItem item)
////            {
////                lock (_syncRoot)
////                {
////                    var itemStore = ItemHolder<TItem>.ItemStore; //get a snapshot of ItemStore

////                    if (itemStore == null)
////                    {
////                        itemStore = new SoleItemStore<TItem>(item); //if the originally ItemStore is null, create a new one
////                    }
////                    else
////                    {
////                        itemStore = itemStore.Count == 1
////                            ? new PolyItemStore<TItem>(itemStore.GetFirst()) //if the originally ItemStore is a SoleItemStore (only has 1 item), upgrade it to a PolyItemStore.
////                            : itemStore.DeepClone(); //if it is a PolyItemStore, get a clone of it. 
////                        itemStore.Add(item);
////                    }

////                    ItemHolder<TItem>.ItemStore = itemStore;
////                }
////            }

////            public override bool Remove<TItem>(TItem item)
////            {
////                lock (_syncRoot)
////                {
////                    var itemStore = ItemHolder<TItem>.ItemStore;
////                    if (itemStore == null)
////                        return false;

////                    itemStore = itemStore.DeepClone();
////                    if (!itemStore.Remove(item))
////                        return false;

////                    if (itemStore.Count == 0)
////                    {
////                        ItemHolder<TItem>.ItemStore = null;
////                    }
////                    else if (itemStore.Count == 1)
////                    {
////                        itemStore = new SoleItemStore<TItem>(itemStore.GetFirst());
////                        ItemHolder<TItem>.ItemStore = itemStore;
////                    }
////                    else
////                    {
////                        ItemHolder<TItem>.ItemStore = itemStore;
////                    }

////                    return true;
////                }
////            }

////            public override bool Remove<TDerived>(TypedItem<TDerived> typedItem)
////            {
////                var item = typedItem as TDerived;
////                return Remove<TDerived>(item);
////            }

////            public override Exception TryGetFirstValue<TItem>(out TItem item)
////            {
////                var itemStore = ItemHolder<TItem>.ItemStore;

////                if (itemStore == null)
////                {
////                    item = null;
////                    return GetKeyNotFoundException("");
////                }

////                item = itemStore.GetFirst();
////                return item == null 
////                    ? GetKeyNotFoundException("") 
////                    : null;
////            }
////        }

////        public abstract void Add<TItem>(TItem item) where TItem : TBaseTypedItem;
////        public abstract bool Remove<TItem>(TItem item) where TItem : TBaseTypedItem;
////        public abstract Exception TryGetFirstValue<TItem>(out TItem item) where TItem : TBaseTypedItem;
////    }

////    public abstract partial class MultiTypedMap
////    {
////        protected readonly object _syncRoot = new object();
////        Dictionary<Type, AbstractItemStore<TBaseTypedItem>> _typedItems;

////        protected void AddTypedItem(Type type, TBaseTypedItem typedItem)
////        {
////        }

////        protected void RemoveTypedItem(Type type, TBaseTypedItem typedItem)
////        {
////        }

////        protected Exception GetKeyNotFoundException(string errMsg)
////        {
////            return new KeyNotFoundException(GetType().MetadataToken, errMsg);
////        }

////        public override void Clear()
////        {
////            lock (_syncRoot)
////            {
////                if (_typedItems.Count == 0)
////                    return;
////                var items = new List<TBaseTypedItem>();
////                foreach (var baseTypedItem in _typedItems)
////                    items.AddRange(baseTypedItem.Value.GetAll());
////                foreach (var item in items)
////                    item.Remove(this);
////            }
////        }

////        public override bool ContainsKey(Type type)
////        {
////            lock (_syncRoot)
////                return _typedItems.ContainsKey(type);
////        }
////    }

////    abstract partial class MultiTypedMap : AbstractTypedMap
////    {
////        static readonly RandomTypeProvider TypeProvider = new RandomTypeProvider();

////        public static MultiTypedMap Create()
////        {
////            Type type1, type2;
////            TypeProvider.GetNextGenericArgumentTypes(out type1, out type2);
////            var closedMapType = typeof(InnerTypedMap<,>).MakeGenericType(type1, type2);
////            return Activator.CreateInstance(closedMapType) as MultiTypedMap;
////        }
////    }
//}
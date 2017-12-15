//using System;
//using System.Threading;

//namespace JointCode.Common.Collections
//{
//    partial class UntrackableTypedMap
//    {
//        /// <summary>
//        /// Internal implementation.
//        /// </summary>
//        /// <remarks>
//        /// Two extra generic parameters (<typeparamref name="TDistrinct1"/> and <typeparamref name="TDistrinct2"/>) are introduced on purpose, 
//        /// so that we can build a new different closed generic subclass of <see cref="UntrackableTypedMap"/> by only changing the <typeparamref name="TDistrinct1"/> and <typeparamref name="TDistrinct2"/>.
//        /// </remarks>
//        class InnerTypedMap<TDistrinct1, TDistrinct2> : UntrackableTypedMap
//        {
//            /// By this class, we use a tricky that:
//            /// 1. CLR does not share static fields between different generic types
//            /// 2. CLR does not erase types ever.
//            static class ItemHolder<TItem> where TItem : class
//            {
//                internal static TItem Item;
//            }

//            public override bool TryAdd<TItem>(TItem item)
//            {
//                if (Interlocked.CompareExchange(ref ItemHolder<TItem>.Item, item, null) != null)
//                    return false;
//                Count += 1;
//                return true;
//            }

//            public override bool Remove<TItem>(TItem item) //where TItem : class 
//            {
//                if (Interlocked.CompareExchange(ref ItemHolder<TItem>.Item, null, item) != item)
//                    return false;
//                Count -= 1;
//                return true;
//            }

//            public override TItem GetValue<TItem>()
//            {
//                return ItemHolder<TItem>.Item;
//            }
//        }
//    }

//    /// <summary>
//    /// A <see cref="UntrackableTypedMap"/> is a fast, thread safe map between a type and an value of that type. 
//    /// Note that this class won't track items that it holds. The user is totally responsible for item management (Add/Remove) themselves. 
//    /// If they forget to remove unnecessary items from the map explicitly, then these items will live as long as <see cref="AppDomain"/> 
//    /// lives, and that means memory leaks, even if the <see cref="UntrackableTypedMap"/> itself is collected.
//    /// </summary>
//    /// <example>
//    /// The following code shows the usage of this class.
//    /// <code>
//    /// class InstanceCreatorTypedMap
//    /// {
//    ///     readonly UnsafeTypedMap _typedMap;
//    ///     internal InstanceCreatorTypedMap()
//    ///     {
//    ///         _typedMap = UnsafeTypedMap.Create();
//    ///     }
//    ///     public bool TryAdd{TContract}(InstanceCreator{TContract} value)
//    ///     {
//    ///         return _typedMap.TryAdd{InstanceCreator{TContract}}(value);
//    ///     }
//    ///     public bool Remove{TContract}(InstanceCreator{TContract} value)
//    ///     {
//    ///         return _typedMap.Remove{InstanceCreator{TContract}}(value);
//    ///     }
//    ///     public InstanceCreator{TContract} GetValue{TContract}()
//    ///     {
//    ///         return _typedMap.GetValue{InstanceCreator{TContract}}();
//    ///     }
//    /// }
//    /// public class InstanceCreator{TContract}
//    /// {
//    ///     readonly Type _conreteType;
//    ///     public InstanceCreator(Type conreteType)
//    ///     {
//    ///         if (!Key.IsAssignableFrom(conreteType))
//    ///             throw new ArgumentException("Invalid Type!");
//    ///         _conreteType = conreteType;
//    ///     }
//    ///     public Type Key
//    ///     {
//    ///         get { return typeof(TContract); }
//    ///     }
//    ///     public TContract Create()
//    ///     {
//    ///         return (TContract)Activator.CreateInstance(_conreteType);
//    ///     }
//    /// }
//    /// </code>
//    /// </example>
//    /// <remarks>
//    /// It use a tricky that CLR does not share static fields between different generic types and it does not erase types.
//    /// Because the internal data structure uses static fields to store values, the user must remove unnecessary items from the map
//    /// explicitly to correctly release the memory those items take.
//    /// </remarks>
//    public abstract partial class UntrackableTypedMap
//    {
//        static readonly AbstractTypedMap.RandomTypeProvider _typeProvider = new AbstractTypedMap.RandomTypeProvider();

//        /// <summary>
//        /// Gets the number of items registered in the map.
//        /// </summary>
//        public int Count { get; protected set; }
//        /// <summary>
//        /// Tries to add an item to the map.
//        /// </summary>
//        /// <typeparam name="TItem">The type of the item.</typeparam>
//        /// <param name="item">The item.</param>
//        /// <example>
//        /// if (!TryAdd{ISomeType}(aDisposableInstance))
//        /// {
//        ///     aDisposableInstance.Dispose();
//        /// }
//        /// </example>
//        /// <returns>
//        /// A boolean indicating whether the operation is successful. If it is not, then the user is responsible 
//        /// for releasing the unnecessary resource. See the code example above.
//        /// </returns>
//        public abstract bool TryAdd<TItem>(TItem item) where TItem : class;
//        /// <summary>
//        /// Gets the item registered into the map with type <typeparamref name="TItem"/>.
//        /// </summary>
//        /// <typeparam name="TItem">The type of the item.</typeparam>
//        /// <returns>The registered item if one is found, or null.</returns>
//        public abstract TItem GetValue<TItem>() where TItem : class;
//        /// <summary>
//        /// Removes the item registered with the type <typeparamref name="TItem"/> from the map.
//        /// </summary>
//        /// <typeparam name="TItem">The type of the item.</typeparam>
//        /// <param name="item">The item.</param>
//        /// <returns></returns>
//        public abstract bool Remove<TItem>(TItem item) where TItem : class;

//        /// <summary>
//        /// Creates an instance of <see cref="UntrackableTypedMap"/>, for which the user is responsible for 
//        /// item management (Add/Remove) themselves.
//        /// </summary>
//        /// <returns></returns>
//        public static UntrackableTypedMap Create()
//        {
//            Type type1, type2;
//            _typeProvider.GetNextGenericArgumentTypes(out type1, out type2);
//            var closedMapType = typeof(InnerTypedMap<,>).MakeGenericType(type1, type2);
//            return Activator.CreateInstance(closedMapType) as UntrackableTypedMap;
//        }
//    }
//}
//using System;
//using System.Collections.Generic;

//namespace JointCode.Common.Collections
//{
//    public abstract class AbstractTypedMap : CriticalDisposable
//    {
//        protected class RandomTypeProvider
//        {
//            int _typeIndex1;
//            int _typeIndex21;
//            int _typeIndex22;
//            readonly object _syncRoot = new object();
//            // concrete reference types
//            static readonly List<Type> _concreteRefTypes;

//            static RandomTypeProvider()
//            {
//                _concreteRefTypes = new List<Type>();
//                var types = typeof(object).Assembly.GetTypes();
//                foreach (var type in types)
//                {
//                    // skip the open generic types and value types, because:
//                    // 1. an open generic type can not be generic parameter for other open generic types
//                    // 2. if we use value types as generic parameter, this will make the CLR to create unique 
//                    //    type for each one of them at runtime, thus unable to share code
//                    if (type.IsValueType || type.ContainsGenericParameters || type.IsGenericTypeDefinition
//                        || type.IsPointer || type.IsCOMObject || type.IsImport || type.IsSpecialName || type.Name.StartsWith("_"))
//                        continue;
//                    _concreteRefTypes.Add(type);
//                }
//            }

//            internal void GetNextGenericArgumentType(out Type type1)
//            {
//                type1 = _concreteRefTypes[GetTypeIndexSafely()];
//            }
//            int GetTypeIndexSafely()
//            {
//                lock (_syncRoot)
//                {
//                    if (_typeIndex1 < _concreteRefTypes.Count)
//                        return _typeIndex1++;
//                    _typeIndex1 = 0;
//                    return 0;
//                }
//            }

//            internal void GetNextGenericArgumentTypes(out Type type1, out Type type2)
//            {
//                int index1, index2;
//                GetTypeIndexSafely(out index1, out index2);
//                type1 = _concreteRefTypes[index1];
//                type2 = _concreteRefTypes[index2];
//            }
//            void GetTypeIndexSafely(out int index1, out int index2)
//            {
//                lock (_syncRoot)
//                {
//                    if (_typeIndex21 < _concreteRefTypes.Count)
//                    {
//                        index1 = _typeIndex21++;
//                    }
//                    else
//                    {
//                        _typeIndex21 = 0;
//                        index1 = 0;
//                    }
//                    if (_typeIndex22 < _concreteRefTypes.Count)
//                    {
//                        index2 = _typeIndex22++;
//                    }
//                    else
//                    {
//                        _typeIndex22 = 0;
//                        index2 = 0;
//                    }
//                }
//            }
//        }
//    }
//}
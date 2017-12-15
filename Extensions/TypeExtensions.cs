//
// Authors:
//   刘静谊 (Johnny Liu) <jingeelio@163.com>
//
// Copyright (c) 2017 刘静谊 (Johnny Liu)
//
// Licensed under the LGPLv3 license. Please see <http://www.gnu.org/licenses/lgpl-3.0.html> for license text.
//

using System;
using System.Reflection;

#if ENCRYPT
namespace JointCode.Internals
#else
using JointCode.Common.Helpers;
namespace JointCode.Common.Extensions
#endif
{
    public static class TypeExtensions
    {
        static readonly Type TypeBuilderInstantiation;

        static TypeExtensions()
        {
            TypeBuilderInstantiation = Type.GetType("System.Reflection.Emit.TypeBuilderInstantiation");
        }

        public static bool IsConstructedGeneric(this Type type)
        {
            var underlyingType = type.GetType();
            return underlyingType == TypeBuilderInstantiation;
        }

        public static bool IsPublic(this Type type)
        {
            if (!type.IsNested)
                return type.IsPublic;
            if (!type.IsNestedPublic)
                return false;

            var declaringType = type.DeclaringType;
            do
            {
                if (!declaringType.IsNested)
                    return declaringType.IsPublic;
                if (!declaringType.IsNestedPublic)
                    return false;
                declaringType = declaringType.DeclaringType;
            } while (true);
        }

        public static bool IsConcrete(this Type type)
        {
            return !type.IsInterface && !type.IsAbstract;
        }

        public static bool HasDefaultConstructor(this Type type)
        {
            return type.GetConstructor(Type.EmptyTypes) != null;
        }

        //public static bool IsAssignableFromGeneric(this Type openGenericBaseType, Type openGenericSubType)
        //{
        //    Requires.Instance.NotNull(openGenericBaseType, "openGenericBaseType");
        //    Requires.Instance.NotNull(openGenericSubType, "openGenericSubType");
        //    Requires.Instance.IsOpenGenericType(openGenericBaseType, "openGenericBaseType");
        //    Requires.Instance.IsOpenGenericType(openGenericSubType, "openGenericSubType");
        //    // The (openGenericBaseType == openGenericSubType) won't work for the generic types obtained by 
        //    // calculation [like Type.GetGenericArguments()], because type.FullName might return null.
        //    if (ReferenceEquals(openGenericBaseType.Module, openGenericSubType.Module) 
        //        && openGenericBaseType.MetadataToken == openGenericSubType.MetadataToken)
        //        return true;
        //    if (openGenericBaseType.IsInterface)
        //    {
        //        var interfaces = openGenericSubType.GetInterfaces();
        //        foreach (var @interface in interfaces)
        //        {
        //            if (@interface.IsGenericType && @interface.GetGenericTypeDefinition() == openGenericBaseType)
        //                return true;
        //        }
        //    }
        //    else
        //    {
        //        var baseType = openGenericSubType.BaseType;
        //        while (baseType != null)
        //        {
        //            if (baseType.IsGenericType && openGenericBaseType == baseType.GetGenericTypeDefinition())
        //                return true;
        //            baseType = baseType.BaseType;
        //        }
        //    }
        //    return false;
        //}

        public static bool IsAssignableFromEx(this Type baseType, Type subType)
        {
            Requires.Instance.NotNull(baseType, "baseType");
            Requires.Instance.NotNull(subType, "subType");

            if (baseType.IsGenericTypeDefinition)
            {
                // 如果基类是开放泛型类，则派生类也必定是开放泛型类
                Requires.Instance.IsOpenGenericType(subType, "subType");
                // The (baseType == subType) won't work for the generic types obtained by 
                // calculation [like Type.GetGenericArguments()], because its TypeHandle is different, and the type.FullName might return null.
                if (ReferenceEquals(baseType.Module, subType.Module)
                    && baseType.MetadataToken == subType.MetadataToken)
                    return true;

                if (baseType.IsInterface)
                {
                    var interfaces = subType.GetInterfaces();
                    foreach (var @interface in interfaces)
                    {
                        if (@interface.IsGenericType && @interface.GetGenericTypeDefinition() == baseType)
                            return true;
                    }
                }
                else
                {
                    var childType = subType.BaseType;
                    while (childType != null)
                    {
                        if (childType.IsGenericType && baseType == childType.GetGenericTypeDefinition())
                            return true;
                        childType = childType.BaseType;
                    }
                }
            }
            else
            {
                // 即使基类不是开放泛型类，派生类也可以是开放泛型类（例如开放泛型类实现了一个接口）
                if (ReferenceEquals(baseType, subType))
                    return true;

                if (baseType.IsInterface)
                {
                    var interfaces = subType.GetInterfaces();
                    foreach (var @interface in interfaces)
                    {
                        if (@interface == baseType)
                            return true;
                    }
                }
                else
                {
                    var childType = subType.BaseType;
                    while (childType != null)
                    {
                        if (baseType == childType)
                            return true;
                        childType = childType.BaseType;
                    }
                }
            }
            
            // 其他未预料到的情况
            return false;
        }

        public static string ToTypeNameOnly(this Type type)
        {
            Requires.Instance.NotNull(type, "type");
            if (type.IsArray)
                return type.GetElementType().ToTypeNameOnly() + "[]";

            var typeName = type.Name;
            return !type.IsGenericType
                ? typeName
                : typeName.Substring(0, typeName.IndexOf('`'));
        }

        public static string ToTypeName(this Type type)
        {
            Requires.Instance.NotNull(type, "type");
            if (type.IsArray)
                return type.GetElementType().ToTypeName() + "[]";

            var typeName = (type.IsNested && !type.IsGenericParameter)
                ? type.DeclaringType.ToTypeName() + "+" + type.Name
                : type.Name;

            if (!type.IsGenericType)
                return typeName;

            typeName = typeName.Substring(0, typeName.IndexOf('`'));
            var genericArguments = type.GetGenericArguments();
            var argumentNames = new string[genericArguments.Length];

            for (var i = 0; i < genericArguments.Length; i++)
                argumentNames[i] = genericArguments[i].ToTypeName();

            return typeName + "<" + string.Join(", ", argumentNames) + ">";
        }

        public static string ToFullTypeName(this Type type)
        {
            Requires.Instance.NotNull(type, "type");
            if (type.IsArray)
                return type.GetElementType().ToFullTypeName() + "[]";

            var typeName = (type.IsNested && !type.IsGenericParameter)
                ? type.DeclaringType.ToFullTypeName() + "+" + type.Name
                : type.Namespace + "." + type.Name; // type.FullName might return null for the types obtained by calculation [like Type.GetGenericArguments()].

            if (!type.IsGenericType)
                return typeName;

            typeName = typeName.Substring(0, typeName.IndexOf('`'));
            var genericArguments = type.GetGenericArguments();
            var argumentNames = new string[genericArguments.Length];

            for (var i = 0; i < genericArguments.Length; i++)
                argumentNames[i] = genericArguments[i].ToFullTypeName();

            return typeName + "<" + string.Join(", ", argumentNames) + ">";
        }

        public static Type GetSoleGenericArgument(this Type type)
        {
            Requires.Instance.NotNull(type, "type");
            Requires.Instance.IsGenericType(type, "type");
            var genParams = type.GetGenericArguments();
            Requires.Instance.True(genParams.Length == 1,
                string.Format("The type [{0}] contains multiple generic arguments!", type.ToFullTypeName()));
            return genParams[0];
        }

        public static bool MeetGenericConstaints(this Type formalParam, Type actualParam)
        {
            Requires.Instance.NotNull(formalParam, "formalParam");
            Requires.Instance.NotNull(actualParam, "actualParam");
            Requires.Instance.True(formalParam.IsGenericParameter, "The formal parameter must be a generic parameter!");

            // Check for the presence of class, struct or new() constraints
            var attribs = formalParam.GenericParameterAttributes; 
            if (attribs != GenericParameterAttributes.None)
            {
                if ((attribs & GenericParameterAttributes.ReferenceTypeConstraint) != 0)
                {
                    if (actualParam.IsValueType)
                        return false;
                }
                if ((attribs & GenericParameterAttributes.NotNullableValueTypeConstraint) != 0)
                {
                    if (actualParam.IsClass)
                        return false;
                }
                if ((attribs & GenericParameterAttributes.DefaultConstructorConstraint) != 0)
                {
                    var ctor = actualParam.GetConstructor(Type.EmptyTypes);
                    if (ctor == null)
                        return false;
                }
            }

            // This gives you an array of types that present the type constraints on the generic argument T
            var constraintTypes = formalParam.GetGenericParameterConstraints(); 
            if (constraintTypes.Length > 0)
            {
                for (int i = 0; i < constraintTypes.Length; i++)
                {
                    var constraintType = constraintTypes[i];
                    if (!constraintType.IsAssignableFrom(actualParam))
                        return false;
                }
            }
            else
            {
                // generic parameter with no constraints.
                return true;
            }

            return true;
        }

        /// <summary>
        /// Make and return the generic type using the given type and generic arguments with respect to the generic constraints.
        /// </summary>
        /// <param name="openGenericType">The open generic type.</param>
        /// <param name="genericArgTypes">The generic arg types.</param>
        /// <returns></returns>
        public static Type MakeGenericTypeEx(this Type openGenericType, params Type[] genericArgTypes)
        {
            Requires.Instance.NotNull(openGenericType, "openGenericType");
            Requires.Instance.True(openGenericType.IsGenericTypeDefinition, "");

            var genArgs = openGenericType.GetGenericArguments();
            Requires.Instance.True(genericArgTypes.Length > 0, "");
            Requires.Instance.True(genArgs.Length == genericArgTypes.Length, "");

            for (int i = 0; i < genArgs.Length; i++)
            {
                if (!genArgs[i].MeetGenericConstaints(genericArgTypes[i]))
                    throw new ArgumentException("");
            }

            return openGenericType.MakeGenericType(genericArgTypes);
        }

        #region GetCompatibleConstructor
        /// <summary>
        /// Tries to get a constructor with the given parameters.
        /// If an exact match is not found, tries to search a compatible one.
        /// If acceptAConstructorCompatibleByCast is true, you can ask for a constructor(object) and
        /// receive a constructor(int), because with a cast that should be valid.
        /// If none is found, return null.
        /// </summary>
        public static ConstructorInfo GetCompatibleConstructor(this Type type, bool acceptAConstructorCompatibleByCast, bool areStringsAndEnumsCompatible, params Type[] parameterTypes)
        {
            if (type == null)
                throw new ArgumentNullException("type");

            if (parameterTypes == null)
                throw new ArgumentNullException("parameterTypes");

            ConstructorInfo result = type.GetConstructor(parameterTypes);
            if (result != null)
                return result;

            var constructors = type.GetConstructors();
            return _CompareParameters(null, acceptAConstructorCompatibleByCast, areStringsAndEnumsCompatible, parameterTypes, constructors);
        }
        #endregion

        #region GetCompatibleMethod
        /// <summary>
        /// Tries to get a method with the given name and parameters.
        /// If an exact match is not found, tries to search a compatible one.
        /// If acceptAMethodCompatibleByCast is true, you can ask for a Method(object) and
        /// receive a Method(int), because with a cast that should be valid.
        /// If none is found, return null.
        /// </summary>
        public static MethodInfo GetCompatibleMethod(this Type type, string name, BindingFlags bindingFlags, bool acceptAMethodCompatibleByCast, bool areStringsAndEnumsCompatible, params Type[] parameterTypes)
        {
            if (type == null)
                throw new ArgumentNullException("type");

            if (name == null)
                throw new ArgumentNullException("name");

            if (parameterTypes == null)
                throw new ArgumentNullException("parameterTypes");

            var method = type.GetMethod(name, bindingFlags, null, parameterTypes, null);
            if (method != null)
                return method;

            var methods = type.GetMethods(bindingFlags);
            return _CompareParameters(name, acceptAMethodCompatibleByCast, areStringsAndEnumsCompatible, parameterTypes, methods);
        }
        #endregion

        #region _CompareParameters
        static T _CompareParameters<T>(string name, bool acceptAMethodCompatibleByCast, bool areStringsAndEnumsCompatible, Type[] parameterTypes, T[] methods)
            where T : MethodBase
        {
            foreach (var methodInfo in methods)
            {
                if (name != null)
                    if (methodInfo.Name != name)
                        continue;

                var methodParameterTypes = methodInfo.GetParameterTypes();
                if (methodParameterTypes.Length != parameterTypes.Length)
                    continue;

                bool ok = true;
                for (int i = 0; i < parameterTypes.Length; i++)
                {
                    var methodParameterType = methodParameterTypes[i];
                    var parameterType = parameterTypes[i];

                    if (parameterType.IsGenericParameter || methodParameterType.IsGenericParameter)
                        continue;

                    if (!methodParameterType.IsAssignableFrom(parameterType))
                    {
                        if (acceptAMethodCompatibleByCast)
                        {
                            if (parameterType.IsAssignableFrom(methodParameterType))
                                continue;

                            if (methodParameterType.IsByRef && parameterType.IsByRef)
                            {
                                var element1 = parameterType.GetElementType();
                                var element2 = methodParameterType.GetElementType();

                                if (element1.IsGenericParameter || element2.IsGenericParameter)
                                    continue;

                                if (element1.IsAssignableFrom(element2) || element2.IsAssignableFrom(element1))
                                    continue;
                            }
                        }

                        if (areStringsAndEnumsCompatible)
                        {
                            if (methodParameterType == typeof(string) && parameterType.IsEnum)
                                continue;

                            if (parameterType == typeof(string) && methodParameterType.IsEnum)
                                continue;
                        }

                        ok = false;
                        break;
                    }
                }

                if (ok)
                    return methodInfo;
            }

            return null;
        }
        #endregion
    }
}

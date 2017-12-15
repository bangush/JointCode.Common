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
    public static class MethodBaseExtensions
    {
        static readonly Type MethodBuilderInstantiation;

        static MethodBaseExtensions()
	    {
	        MethodBuilderInstantiation = Type.GetType("System.Reflection.Emit.MethodBuilderInstantiation");
        }

        /// <summary>
        /// Gets an array with the parameter types of this method info.
        /// </summary>
        public static Type[] GetParameterTypes(this MethodBase method)
        {
            if (method == null)
                throw new ArgumentNullException("method");

            //如果 method.IsGenericMethodDefinition，以下方法将会报错。
            var parameters = method.GetParameters();

            if (parameters.Length == 0)
                return Type.EmptyTypes;

            var result = new Type[parameters.Length];
            for (int i = 0; i < parameters.Length; i++)
                result[i] = parameters[i].ParameterType;
            return result;
        }

        /// <summary>
        /// Make and return the generic method using the given method and generic arguments with respect to the generic constraints.
        /// </summary>
        /// <param name="openGenericMethod">The open generic method.</param>
        /// <param name="genericArgTypes">The generic arg types.</param>
        /// <returns></returns>
        public static MethodInfo MakeGenericMethodEx(this MethodInfo openGenericMethod, params Type[] genericArgTypes)
        {
            Requires.Instance.NotNull(openGenericMethod, "openGenericMethod");
            Requires.Instance.True(openGenericMethod.IsGenericMethodDefinition, "");

            var genArgs = openGenericMethod.GetGenericArguments();
            Requires.Instance.True(genericArgTypes.Length > 0, "");
            Requires.Instance.True(genArgs.Length == genericArgTypes.Length, "");

            for (int i = 0; i < genArgs.Length; i++)
            {
                if (!genArgs[i].MeetGenericConstaints(genericArgTypes[i]))
                    throw new ArgumentException("");
            }

            return openGenericMethod.MakeGenericMethod(genericArgTypes);
        }

        public static bool IsConstructedGeneric(this MethodInfo method)
        {
            var underlyingType = method.GetType();
            return underlyingType == MethodBuilderInstantiation;
        }
    }
}

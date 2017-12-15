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
namespace JointCode.Common.Extensions
#endif
{
    /// <summary>
    /// Adds the GetTypes method to a parameterInfo array.
    /// </summary>
    public static class ParameterInfoExtensions
	{
        static readonly Type GenericTypeParameterBuilder;

        static ParameterInfoExtensions()
        {
            GenericTypeParameterBuilder = Type.GetType("System.Reflection.Emit.GenericTypeParameterBuilder");
        }

        public static bool IsConstructedGeneric(this ParameterInfo parameter)
        {
            var underlyingType = parameter.GetType();
            return underlyingType == GenericTypeParameterBuilder;
        }

		/// <summary>
		/// Returns an arrays that contains all the parameterTypes of the parameterInfos array.
		/// </summary>
        public static Type[] GetTypes(this ParameterInfo[] parameters)
		{
            if (parameters == null)
                throw new ArgumentNullException("parameters");

            if (parameters.Length == 0)
                return Type.EmptyTypes;

            var result = new Type[parameters.Length];
            for (int i = 0; i < parameters.Length; i++)
                result[i] = parameters[i].ParameterType;

			return result;
		}

        public static bool IsRef(this ParameterInfo parameter)
        {
            return parameter.ParameterType.IsByRef;
        }

        public static bool IsRefOrOut(this ParameterInfo parameter)
        {
            return parameter.ParameterType.IsByRef || parameter.IsOut;
        }

        public static Type GetBareParameterType(this ParameterInfo parameter)
        {
            return parameter.ParameterType.IsByRef || parameter.IsOut
                ? parameter.ParameterType.GetElementType()
                : parameter.ParameterType;
        }
	}
}

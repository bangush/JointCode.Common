using System;
using System.Reflection;

#if ENCRYPT
namespace JointCode.Internals
#else
using JointCode.Common.Helpers;
namespace JointCode.Common.Extensions
#endif
{
    /// <summary>
    /// Adds some useful methods to the MemberInfo and Type types to
    /// work easily with attributes (Generics).
    /// </summary>
    public static class AttributeExtensions
    {
        #region GetCustomAttribute

        /// <summary>
        /// Gets an attribute of the specified type, or null.
        /// This is useful when the attribute has AllowMultiple=false, but
        /// don't use it if the class can have more than one attribute of such
        /// type, as this method throws an exception when this happens.
        /// </summary>
        /// <typeparam name="T">The type of the parameter to find.</typeparam>
        /// <param name="attribProvider">The attribute provider where the attributes will be searched.</param>
        /// <param name="inherit">true to search in base classes for attributes that support inheritance.</param>
        /// <returns>The found attribute or null.</returns>
        public static T GetCustomAttribute<T>(this ICustomAttributeProvider attribProvider, bool inherit)
            where T : Attribute
        {
            return GetCustomAttribute<T>(attribProvider, inherit, true);
        }

        /// <summary>
        /// Gets an attribute of the specified type, or null.
        /// This is useful when the attribute has AllowMultiple=false, but
        /// don't use it if the class can have more than one attribute of such
        /// type, as this method throws an exception when this happens.
        /// </summary>
        /// <typeparam name="T">The type of the parameter to find.</typeparam>
        /// <param name="attribProvider">The attribute provider where the attributes will be searched.</param>
        /// <param name="inherit">true to search in base classes for attributes that support inheritance.</param>
        /// <param name="throwWhenMultipleAttribFound">if set to <c>true</c>, throw when multiple attribute found.</param>
        /// <returns>
        /// The found attribute or null.
        /// </returns>
        /// <exception cref="ApplicationConfigurationException">There is more than one attribute of type " + typeof(T).FullName + ".</exception>
        public static T GetCustomAttribute<T>(this ICustomAttributeProvider attribProvider, bool inherit, bool throwWhenMultipleAttribFound)
            where T : Attribute
        {
            var attributes = attribProvider.GetCustomAttributes(typeof(T), inherit);
            if (attributes.Length == 0)
                return null;
            if (attributes.Length == 1)
                return (T)attributes[0];

            if (throwWhenMultipleAttribFound)
                throw ExceptionHelper.HandleAndReturn(
                    new ApplicationConfigurationException("There is more than one attribute of type " + typeof(T).FullName + "."));
            return (T)attributes[0];
        }

        #endregion
    }
}

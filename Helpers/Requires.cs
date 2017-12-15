//
// Authors:
//   刘静谊 (Johnny Liu) <jingeelio@163.com>
//
// Copyright (c) 2017 刘静谊 (Johnny Liu)
//
// Licensed under the LGPLv3 license. Please see <http://www.gnu.org/licenses/lgpl-3.0.html> for license text.
//

using System;
using System.Diagnostics;
using System.Globalization;
using JointCode.Common.Extensions;

namespace JointCode.Common.Helpers
{
	// 契约式编程要求我们在『前提条件』、『后继条件』和『不变量条件』进行契约检查。
	// 前置条件发生在每个操作（方法，或者函数）的最开始，后置条件发生在每个操作的最后，不变式实际上是前置条件和后置条件的交集。违反这些操作会导致程序抛出异常。
    // 首先应该知道，异常用于在执行条件不满足时，及时中断当前程序执行流，避免出现更大的错误。
    // 而异常处理机制（抛出异常）的目的则在于让应用程序决定是否能够从异常中恢复并继续执行；
    // 如果不能恢复，也可以优雅地退出应用程序（例如进行一些异常记录），而不是简单粗暴地中断应用程序。
    // 我们可以简单将异常分为以下几种类型：
    // 1. 不会对内部状态造成破坏的异常。在 JointCode 的代码中，除了继承自 <see cref="FatalException"/> 和 <see cref="LogicalException"/> 之外的
    //    异常都属于此类。例如，参数错误、IO 错误等异常。对于这类异常，应用程序可以从异常中恢复并继续执行。
    // 2. 可能会对内部状态造成破坏的异常，在 JointCode 的代码中，对应于继承自 <see cref="LogicalException"/> 的异常。例如，无效操作、数据错误等
    //    异常。对于这类异常，应用程序可以选择从异常中恢复并继续执行，也可以直接中断应用程序执行。
    // 3. 肯定会对内部状态造成破坏的异常，在 JointCode 的代码中，对应于继承自 <see cref="FatalException"/> 的异常。例如，资源耗竭、堆栈溢出、访问
    //    违规等异常。对于这类异常，应用程序不应从异常中恢复，而应中断应用程序运行。
    // 我们可以简单将异常分为以下几种类型：
    // 1.不会对内部状态造成破坏的异常。例如，参数错误、IO 错误等异常。对于这类异常，应用程序可以从异常中恢复并继续执行。
    // 2.可能会对内部状态造成破坏的异常。例如，无效操作、数据错误等异常。对于这类异常，应用程序可以选择从异常中恢复并继续执行，也可以直接中断应用程序执行。
    // 3.肯定会对内部状态造成破坏的异常。例如，资源耗竭、堆栈溢出、访问违规、等异常。对于这类异常，应用程序不应从异常中恢复，而应直接中断应用程序运行。
    // 不同类型的异常应该继承自不同的异常基类，以便能够通过 <see cref="IExceptionManager"/> 和 <see cref="IExceptionHandler"/> 进行适当的处理。

    /// <summary>
    /// Design by contract
    /// </summary>
    public class Requires
    {
        static readonly Requires _instance = new Requires();

        private Requires() { }

        public static Requires Instance { get { return _instance; } }

        public void True(bool value, string message)
        {
            if (!value)
                throw ExceptionHelper.HandleAndReturn(new PreconditionException(message));
        }

        public void NotNull(object value, string paramName)
        {
            if (value == null)
                throw ExceptionHelper.HandleAndReturn(new PreconditionException(string.Format(CultureInfo.InvariantCulture,
                    "The [{0}] can not be null!", paramName)));
        }

        public void NotNullOrEmpty(string value, string paramName)
        {
            if (string.IsNullOrEmpty(value))
                throw ExceptionHelper.HandleAndReturn(new PreconditionException(string.Format(CultureInfo.InvariantCulture,
                    "The [{0}] can not be null or empty!", paramName)));
        }

        public void NotNullOrWhiteSpace(string value, string paramName)
        {
            if (value.IsNullOrWhiteSpace())
                throw ExceptionHelper.HandleAndReturn(new PreconditionException(string.Format(CultureInfo.InvariantCulture,
                    "The [{0}] can not be null or whitespace!", paramName)));
        }

        [Conditional("DEBUG")]
        public void Assert(bool value, string message)
        {
            if (!value)
                throw ExceptionHelper.HandleAndReturn(new AssertionException(message));
        }

        [Conditional("DEBUG")]
        public void EnsureTrue(bool value, string message)
        {
            if (!value)
                throw ExceptionHelper.HandleAndReturn(new PostconditionException(message));
        }

        [Conditional("DEBUG")]
        public void EnsureNotNull(object value, string paramName)
        {
            if (value == null)
                throw ExceptionHelper.HandleAndReturn(new PostconditionException(string.Format("The [{0}] can not be null!", paramName)));
        }

        [Conditional("DEBUG")]
        public void EnsureNotNullOrEmpty(string value, string paramName)
        {
            if (string.IsNullOrEmpty(value))
                throw ExceptionHelper.HandleAndReturn(new PostconditionException(string.Format("The [{0}] can not be null or empty!", paramName)));
        }

        [Conditional("DEBUG")]
        public void EnsureNotNullOrWhiteSpace(string value, string paramName)
        {
            if (value.IsNullOrWhiteSpace())
                throw ExceptionHelper.HandleAndReturn(new PostconditionException(string.Format("The [{0}] can not be null or whitespace!", paramName)));
        }

        public void IsPublicType(Type type, string paramName)
        {
            if (!type.IsPublic())
                throw ExceptionHelper.HandleAndReturn(new ArgumentException(string.Format(CultureInfo.InvariantCulture,
                    "The [{0}] is of type [{1}], which is not a public accessible type!", paramName,
                    type.ToFullTypeName()), paramName));
        }

        public void IsConcreteType(Type type, string paramName)
        {
            if (!type.IsConcrete())
                throw ExceptionHelper.HandleAndReturn(new ArgumentException(string.Format(CultureInfo.InvariantCulture,
                    "The [{0}] is of type [{1}], which is not a concrete type!", paramName, type.ToFullTypeName()),
                    paramName));
        }

        public void IsGenericType(Type type, string paramName)
        {
            if (!type.IsGenericType)
                throw ExceptionHelper.HandleAndReturn(new ArgumentException(string.Format(CultureInfo.InvariantCulture,
                    "The [{0}] is of type [{1}], which is not an generic type!", paramName, type.ToFullTypeName()),
                    paramName));
        }

        public void IsNotOpenGenericType(Type type, string paramName)
        {
            // We check for ContainsGenericParameters to see whether there is a Generic Parameter 
            // to find out if this type can be created.
            if (type.ContainsGenericParameters)
                throw ExceptionHelper.HandleAndReturn(new ArgumentException(string.Format(CultureInfo.InvariantCulture,
                    "The [{0}] is of type [{1}], which is an open generic type!", paramName, type.ToFullTypeName()),
                    paramName));
        }

        public void IsOpenGenericType(Type type, string paramName)
        {
            if (!type.ContainsGenericParameters)
                throw ExceptionHelper.HandleAndReturn(new ArgumentException(string.Format(CultureInfo.InvariantCulture,
                    "The [{0}] is of type [{1}], which is not an open generic type!", paramName, type.ToFullTypeName()),
                    paramName));
        }

        public void IsAssignableFrom(Type baseType, Type subType)
        {
            if (baseType != subType && !baseType.IsAssignableFrom(subType))
                throw ExceptionHelper.HandleAndReturn(new ArgumentException(string.Format(CultureInfo.InvariantCulture,
                    "The type [{0}] is not assignable from type [{1}]!",
                    baseType.ToFullTypeName(),
                    subType.ToFullTypeName())));
        }

        public void IsAssignableFromGeneric(Type openGenericBaseType, Type openGenericSubType)
        {
            if (!openGenericBaseType.IsAssignableFromEx(openGenericSubType))
                throw ExceptionHelper.HandleAndReturn(new ArgumentException(string.Format(CultureInfo.InvariantCulture,
                    "The type [{0}] is not assignable from type [{1}]!",
                    openGenericBaseType.ToFullTypeName(),
                    openGenericSubType.ToFullTypeName())));
        }

        public void HasDefaultConstructor(Type type, string paramName)
        {
            if (!type.HasDefaultConstructor())
                throw ExceptionHelper.HandleAndReturn(new ArgumentException(string.Format(CultureInfo.InvariantCulture,
                    "The type [{0}] is supposed to have a default parameterless constructor!", type.ToFullTypeName()),
                    paramName));
        }
    }
}

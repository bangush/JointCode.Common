//
// Authors:
//   刘静谊 (Johnny Liu) <jingeelio@163.com>
//
// Copyright (c) 2017 刘静谊 (Johnny Liu)
//
// Licensed under the LGPLv3 license. Please see <http://www.gnu.org/licenses/lgpl-3.0.html> for license text.
//

using System;
using System.Runtime.Serialization;

namespace JointCode.Common
{
    /// 表示某种肯定会对内部状态造成破坏的异常。对于这类异常，应用程序不应从异常中恢复，而应中断应用程序运行。
    [Serializable]
    public class FatalException : ApplicationException
    {
        public FatalException() { }
        public FatalException(string message) : base(message) { }
        public FatalException(string message, Exception inner) : base(message, inner) { }
        protected FatalException(SerializationInfo info, StreamingContext context)
            : base(info, context) { }
    }

    /// 表示某种可能会对内部状态造成破坏的异常。对于这类异常，应用程序可以选择从异常中恢复并继续执行，也可以直接中断应用程序执行。
    [Serializable]
    public class LogicalException : ApplicationException
    {
        public LogicalException() { }
        public LogicalException(string message) : base(message) { }
        public LogicalException(string message, Exception inner) : base(message, inner) { }
        protected LogicalException(SerializationInfo info, StreamingContext context)
            : base(info, context) { }
    }

    /// 在 JointCode 的代码中，除了继承自 <see cref="FatalException"/> 和 <see cref="LogicalException"/> 的异常之外，其他异常都属于
    /// 不会对内部状态造成破坏的异常。对于这类异常，应用程序可以从异常中恢复并继续执行。

    [Serializable]
    public class PreconditionException : ArgumentException
    {
        public PreconditionException() { }
        public PreconditionException(string message) : base(message) { }
        public PreconditionException(string message, Exception inner) : base(message, inner) { }
        protected PreconditionException(SerializationInfo info, StreamingContext context)
            : base(info, context) { }
    }

    [Serializable]
    public class PostconditionException : ArgumentException
    {
        public PostconditionException() { }
        public PostconditionException(string message) : base(message) { }
        public PostconditionException(string message, Exception inner) : base(message, inner) { }
        protected PostconditionException(SerializationInfo info, StreamingContext context)
            : base(info, context) { }
    }

    [Serializable]
    public class AssertionException : ApplicationException
    {
        public AssertionException() { }
        public AssertionException(string message) : base(message) { }
        public AssertionException(string message, Exception inner) : base(message, inner) { }
        protected AssertionException(SerializationInfo info, StreamingContext context)
            : base(info, context) { }
    }

    [Serializable]
    public class ApplicationInternalException : FatalException
    {
        public ApplicationInternalException() { }
        public ApplicationInternalException(string message) : base(message) { }
        public ApplicationInternalException(string message, Exception inner) : base(message, inner) { }
        protected ApplicationInternalException(SerializationInfo info, StreamingContext context)
            : base(info, context) { }
    }

    [Serializable]
    public class ApplicationConfigurationException : LogicalException
    {
        public ApplicationConfigurationException() { }
        public ApplicationConfigurationException(string message) : base(message) { }
        public ApplicationConfigurationException(string message, Exception inner) : base(message, inner) { }
        protected ApplicationConfigurationException(SerializationInfo info, StreamingContext context)
            : base(info, context) { }
    }

    [Serializable]
    public class InconsistentStateException : LogicalException
    {
        public InconsistentStateException() { }
        public InconsistentStateException(string message) : base(message) { }
        public InconsistentStateException(string message, Exception inner) : base(message, inner) { }
        protected InconsistentStateException(SerializationInfo info, StreamingContext context)
            : base(info, context) { }
    }
}
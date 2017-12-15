//
// Authors:
//   刘静谊 (Johnny Liu) <jingeelio@163.com>
//
// Copyright (c) 2017 刘静谊 (Johnny Liu)
//
// Licensed under the LGPLv3 license. Please see <http://www.gnu.org/licenses/lgpl-3.0.html> for license text.
//

using System;
using System.Collections.Generic;
using System.Diagnostics;

#if ENCRYPT
namespace JointCode.Internals
#else
using JointCode.Common.Extensions;
namespace JointCode.Common.ExceptionHandling
#endif
{
    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class DefaultExceptionHandler : IExceptionHandler
    {
        //该列表也可以通过读取配置文件而创建
        readonly List<IExceptionPolicy> _exceptionPolicies;

        public DefaultExceptionHandler()
        {
            _exceptionPolicies = new List<IExceptionPolicy>();
            AddDefaultExceptionPolicies();
        }

        public DefaultExceptionHandler(bool loadDefaultPolicies)
        {
            _exceptionPolicies = new List<IExceptionPolicy>();
            if (loadDefaultPolicies)
                _exceptionPolicies.Add(new DefaultExceptionPolicy());
        }

        [Conditional("DEBUG")]
        void AddDefaultExceptionPolicies()
        {
            _exceptionPolicies.Add(new DefaultExceptionPolicy());
        }

        /// <summary>
        /// Adds the exception policy.
        /// </summary>
        /// <param name="exceptionPolicy">The exception policy.</param>
        public void AddExceptionPolicy(IExceptionPolicy exceptionPolicy)
        {
            _exceptionPolicies.Add(exceptionPolicy);
        }

        ///// <summary>
        ///// Handles the specified <see cref="Exception"/> object according to the exception classification.
        ///// </summary>
        ///// <param name="originalException">An <see cref="Exception"/> object.</param>
        ///// <param name="rethrowedException">The new <see cref="Exception"/> to throw, if any.</param>
        ///// <returns>
        ///// Whether or not a rethrow is recommended.
        ///// </returns>
        ///// <example>
        ///// The following code shows the usage of the exception handling framework.
        /////   <code>
        ///// try
        ///// {
        /////     DoWork();
        ///// }
        ///// catch (Exception originalException)
        ///// {
        /////     Exception rethrowedException;
        /////     if (ExceptionManager.HandleException(originalException, out rethrowedException))
        /////     {
        /////         if(rethrowedException == null)
        /////             throw;
        /////         else
        /////             throw rethrowedException;
        /////     }
        ///// }
        /////   </code>
        /////   </example>
        //public bool HandleException(Exception originalException, out Exception rethrowedException)
        //{
        //    var preException = originalException;
        //    Exception postException = null;
        //    foreach (var exceptionPolicy in _exceptionPolicies)
        //    {
        //        if (!exceptionPolicy.CanHandle(preException))
        //            continue;

        //        postException = exceptionPolicy.HandleException(preException);
        //        if (postException == null)
        //        {
        //            rethrowedException = null;
        //            return false;
        //        }

        //        preException = postException;
        //    }

        //    rethrowedException = postException;
        //    return postException != null;
        //}

        /// <summary>
        /// Handles the specified <see cref="Exception"/> object according to the exception policies and throw the resulted exception, if one is returned.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <example>
        /// The following code shows the usage of the exception handling framework.
        /// <code>
        /// try
        /// {
        ///     DoWork();
        /// }
        /// catch (Exception exception)
        /// {
        ///     ExceptionManager.HandleException(exception)
        /// }
        /// </code>
        /// </example>
        public void Handle(Exception exception)
        {
            var postException = HandleAndReturn(exception);

            if (postException == null)
                return; // don't throw any exception, since the exception is swallowed by exception policies.

            if (ReferenceEquals(postException, exception)) 
            {
                // rethrow the original exception.
                exception.Rethrow();
            }
            else
            {
                // throw a new exception since a new one is provided.
                throw postException;
            }
        }

        /// <summary>
        /// Handles the specified <see cref="Exception"/> object according to the exception policies and return the resulted exception.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <returns></returns>
        public Exception HandleAndReturn(Exception exception)
        {
            var preException = exception;
            var postException = exception;

            foreach (var exceptionPolicy in _exceptionPolicies)
            {
                if (!exceptionPolicy.CanHandle(preException))
                    continue;

                postException = exceptionPolicy.HandleException(preException);
                if (postException == null)
                    return null; // don't throw any exception.

                preException = postException;
            }

            return postException;
        }
    }
}

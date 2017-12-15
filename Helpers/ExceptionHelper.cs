//
// Authors:
//   刘静谊 (Johnny Liu) <jingeelio@163.com>
//
// Copyright (c) 2017 刘静谊 (Johnny Liu)
//
// Licensed under the LGPLv3 license. Please see <http://www.gnu.org/licenses/lgpl-3.0.html> for license text.
//

using System;
using JointCode.Common.ExceptionHandling;

namespace JointCode.Common.Helpers
{
    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public static class ExceptionHelper
    {
        /// <summary>
        /// Adds the exception policy.
        /// </summary>
        /// <param name="exceptionHandler">The exception handler.</param>
        /// <param name="exceptionPolicy">The exception policy.</param>
        public static void AddExceptionPolicy(IExceptionHandler exceptionHandler, IExceptionPolicy exceptionPolicy)
        {
            exceptionHandler.AddExceptionPolicy(exceptionPolicy);
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
        ///// <code>
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
        ///// </code>
        ///// </example>
        //public static bool HandleException(Exception originalException, out Exception rethrowedException)
        //{
        //    return _exceptionManager.HandleException(originalException, out rethrowedException);
        //}
        /// <summary>
        /// Handles the specified <see cref="Exception"/> according to the exception policies, and throw the resulted exception, 
        /// if one is returned. Or, ignore the exception, if the exception is filtered by policies.
        /// </summary>
        /// <param name="exception">The exception.</param>
        public static void Handle(Exception exception)
        {
            var handler = ExceptionManager.GetExceptionHandler(string.Empty);
            handler.Handle(exception);
        }

        /// <summary>
        /// Handles the specified <see cref="Exception"/> object according to the exception policies and return the resulted exception.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <returns></returns>
        public static Exception HandleAndReturn(Exception exception)
        {
            var handler = ExceptionManager.GetExceptionHandler(string.Empty);
            return handler.HandleAndReturn(exception);
        }
    }
}

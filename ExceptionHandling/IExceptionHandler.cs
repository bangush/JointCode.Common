//
// Authors:
//   刘静谊 (Johnny Liu) <jingeelio@163.com>
//
// Copyright (c) 2017 刘静谊 (Johnny Liu)
//
// Licensed under the LGPLv3 license. Please see <http://www.gnu.org/licenses/lgpl-3.0.html> for license text.
//

using System;

#if ENCRYPT
namespace JointCode.Internals
#else
namespace JointCode.Common.ExceptionHandling
#endif
{
    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public interface IExceptionHandler
    {
        /// <summary>
        /// Adds the exception policy.
        /// </summary>
        /// <param name="exceptionPolicy">The exception policy.</param>
        void AddExceptionPolicy(IExceptionPolicy exceptionPolicy);
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
        //bool HandleException(Exception originalException, out Exception rethrowedException);
        /// <summary>
        /// Handles the specified <see cref="Exception"/> according to the exception policies, and throw the resulted exception, 
        /// if one is returned. Or, ignore the exception, if the exception is filtered by policies.
        /// </summary>
        /// <param name="exception">The exception.</param>
        void Handle(Exception exception);
        /// <summary>
        /// Handles the specified <see cref="Exception"/> object according to the exception policies and return the resulted exception.
        /// Notes that this method might return <code>null</code>.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <returns></returns>
        Exception HandleAndReturn(Exception exception);
    }
}

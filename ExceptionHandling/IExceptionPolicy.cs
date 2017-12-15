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
    /// Represents a policy for handling exceptions.
    /// </summary>
    public interface IExceptionPolicy
    {
        /// <summary>
        /// Gets the name of exception policy.
        /// </summary>
        /// <remarks>This is mainly for configuration purpose.</remarks>
        string Name { get; }

        /// <summary>
        /// Determines whether this instance can handle the specified exception.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <returns>
        ///   <c>true</c> if this instance can handle the specified original exception; otherwise, <c>false</c>.
        /// </returns>
        bool CanHandle(Exception exception);

        /// <summary>
        /// Handles the specified <see cref="Exception"/> object according to the exception classification.
        /// </summary>
        /// <param name="exception">An <see cref="Exception"/> object.</param>
        /// <returns>
        /// The new <see cref="Exception"/> to throw, if any, 
        /// Or the original exception, if it does not need to be replaced by another one.
        /// Or <see langword="null"/>, if no exception will be throwed.
        /// </returns>
        Exception HandleException(Exception exception);
    }
}

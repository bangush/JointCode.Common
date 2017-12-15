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
    /// Defines the contract for an ExceptionHandlerItem.  An ExceptionHandlerItem contains specific handling
    /// logic (i.e. logging the exception, replacing the exception, and so forth.) that is executed in a chain of multiple
    /// ExceptionHandlerItems.  A chain of one or more ExceptionHandlerItems is executed based on the exception type being 
    /// handled, see the <see cref="DefaultExceptionPolicy"/>. 
    /// </summary>    
    public interface IExceptionHandlerItem
    {
        /// <summary>
        /// <para>When implemented by a class, handles an <see cref="Exception"/>.</para>
        /// </summary>
        /// <param name="exception"><para>The exception to handle.</para></param>        
        /// <returns>
        /// <para>Modified exception to be passed to the next exception handler in the chain.</para>
        /// </returns>
        Exception HandleException(Exception exception);
    }
}

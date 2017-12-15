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
using JointCode.Common.Logging;
namespace JointCode.Common.ExceptionHandling
#endif
{
    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class LoggingHandlerItem : IExceptionHandlerItem
    {
        public Exception HandleException(Exception exception)
        {
            //simply log the exception
            var logger = LogManager.GetDefaultLogger();
            logger.Error(exception);
            //throw the original exception
            return exception;
        }
    }
}

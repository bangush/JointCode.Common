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
    class DefaultExceptionPolicy : IExceptionPolicy
    {
        readonly IExceptionHandlerItem _exceptionHandlerItem;

        internal DefaultExceptionPolicy()
        {
            _exceptionHandlerItem = new LoggingHandlerItem();
        }

        public string Name
        {
            get { return GetType().Name; }
        }

        public bool CanHandle(Exception exception)
        {
            return true;
        }

        public Exception HandleException(Exception exception)
        {
            return _exceptionHandlerItem.HandleException(exception);
        }
    }
}

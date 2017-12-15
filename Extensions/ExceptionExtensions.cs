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
namespace JointCode.Common.Extensions
#endif

{
    static class ExceptionExtensions
    {
        class PrivateException : Exception
        {
            internal PrivateException(Exception innerException) 
                : base(null, innerException) { }
        }

        public static void Rethrow(this Exception ex)
        {
//#if NET45
//            ExceptionDispatchInfo.Capture(ex).Throw();
//#elif MONO
//            //Mono version here.
//#else
//            var keepStackTrack = typeof(Exception).GetMethod("PrepForRemoting", BindingFlags.Instance | BindingFlags.NonPublic) 
//                ?? typeof(Exception).GetMethod("InternalPreserveStackTrace", BindingFlags.Instance | BindingFlags.NonPublic);
//            keepStackTrack.Invoke(ex, new object[0]);
//            throw ex;
//#endif
            var e = new PrivateException(ex);
            throw e.InnerException;
        }
    }
}

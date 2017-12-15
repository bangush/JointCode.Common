//
// Authors:
//   刘静谊 (Johnny Liu) <jingeelio@163.com>
//
// Copyright (c) 2017 刘静谊 (Johnny Liu)
//
// Licensed under the LGPLv3 license. Please see <http://www.gnu.org/licenses/lgpl-3.0.html> for license text.
//

using System;

namespace JointCode.Common.Logging
{
    /// <summary>
    /// A logger interface.
    /// </summary>
    public interface ILogger
    {
        string Name { get; }

        void Trace(string message);
        void Trace(Exception exceptioin);
        void TraceFormat(string message, params object[] args);

        void Info(string message);
        void Info(Exception exceptioin);
        void InfoFormat(string message, params object[] args);

        void Debug(string message);
        void Debug(Exception exceptioin);
        void DebugFormat(string message, params object[] args);

        void Warn(string message);
        void Warn(Exception exceptioin);
        void WarnFormat(string message, params object[] args);

        void Error(string message);
        void Error(Exception exceptioin);
        void ErrorFormat(string message, params object[] args);

        void Fatal(string message);
        void Fatal(Exception exceptioin);
        void FatalFormat(string message, params object[] args);
    }
}

//
// Authors:
//   刘静谊 (Johnny Liu) <jingeelio@163.com>
//
// Copyright (c) 2017 刘静谊 (Johnny Liu)
//
// Licensed under the LGPLv3 license. Please see <http://www.gnu.org/licenses/lgpl-3.0.html> for license text.
//

namespace JointCode.Common.Logging
{
    /// <summary>
    /// LogLevel enum
    /// </summary>
    public enum LogLevel : sbyte
    {
        /// <summary>
        /// This is used to log any message defined by user.
        /// </summary>
        Trace = 1,
        /// <summary>
        /// Logs debugging output.
        /// </summary>
        Debug = 2,
        /// <summary>
        /// Logs basic information.
        /// </summary>
        Info = 3,
        /// <summary>
        /// Logs assembly warning.
        /// </summary>
        Warn = 4,
        /// <summary>
        /// Logs an error.
        /// </summary>
        Error = 5,
        /// <summary>
        /// Logs assembly fatal incident.
        /// </summary>
        Fatal = 6,
        /// <summary>
        /// Nothing will be logged.
        /// </summary>
        Off = 7
    }
}

//
// Authors:
//   刘静谊 (Johnny Liu) <jingeelio@163.com>
//
// Copyright (c) 2017 刘静谊 (Johnny Liu)
//
// Licensed under the LGPLv3 license. Please see <http://www.gnu.org/licenses/lgpl-3.0.html> for license text.
//

using System.IO;

using JointCode.Common.Extensions;
using JointCode.Common.Helpers;

namespace JointCode.Common.Logging
{
    public class FileLogSetting
    {
        public FileLogSetting()
            : this(Path.Combine(SystemHelper.AppDirectory, "Log"), SystemHelper.AppName) { }

        public FileLogSetting(string logDirectory, string logFileName)
        {
            Requires.Instance.NotNullOrWhiteSpace(logFileName, "logFileName");
            LogDirectory = logDirectory.IsNullOrWhiteSpace() ? Path.Combine(SystemHelper.AppDirectory, "Log") : logDirectory;
            LogFileName = logFileName;
        }

        public string LogDirectory { get; private set; }
        public string LogFileName { get; private set; }
    }
}

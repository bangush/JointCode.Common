//
// Authors:
//   刘静谊 (Johnny Liu) <jingeelio@163.com>
//
// Copyright (c) 2017 刘静谊 (Johnny Liu)
//
// Licensed under the LGPLv3 license. Please see <http://www.gnu.org/licenses/lgpl-3.0.html> for license text.
//

using System;
using System.Diagnostics;
using System.IO;

namespace JointCode.Common.Helpers
{
    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public static class SystemHelper
    {
        static readonly string _appName = Process.GetCurrentProcess().ProcessName;

        public const int LongSize = sizeof(long) * 8;
        public const int IntSize = sizeof(int) * 8;
        public const int ShortSize = sizeof(short) * 8;
        public const int ByteSize = sizeof(byte) * 8;
        public const int CharSize = sizeof(char) * 8;

        /// <summary>
        /// Gets the application directory.
        /// </summary>
        public static string AppDirectory
        {
            get { return Environment.CurrentDirectory; }
        }

        /// <summary>
        /// Gets the application name.
        /// </summary>
        public static string AppName
        {
            get { return _appName; }
        }

        // If running in 32-bit .NET Framework 2.0 on 64-bit Windows, this will return 32-bit
        public static bool Is64BitApp
        {
            get { return IntPtr.Size == 8; }
        }

        public static bool HasMultiProcessors
        {
            get { return Environment.ProcessorCount > 1; }
        }

        /// <summary>
        /// Gets the image architecture.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <returns>
        /// 0x10b - PE32, i.e, x86
        /// 0x20b - PE32+, i.e, x64
        /// </returns>
        /// <exception cref="BadImageFormatException">
        /// Not a valid Portable Executable image
        /// or
        /// Not a valid Portable Executable image
        /// </exception>
        public static ushort GetImageArchitecture(string filePath)
        {
            using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                using (var reader = new BinaryReader(stream))
                {
                    //check the MZ signature to ensure it's a valid Portable Executable image
                    if (reader.ReadUInt16() != 23117)
                        throw new BadImageFormatException("Not a valid Portable Executable image", filePath);

                    // seek to, and read, e_lfanew then advance the stream to there (start of NT header)
                    stream.Seek(0x3A, SeekOrigin.Current);
                    stream.Seek(reader.ReadUInt32(), SeekOrigin.Begin);

                    // Ensure the NT header is valid by checking the "PE\0\0" signature
                    if (reader.ReadUInt32() != 17744)
                        throw new BadImageFormatException("Not a valid Portable Executable image", filePath);

                    // seek past the file header, then read the magic number from the optional header
                    stream.Seek(20, SeekOrigin.Current);
                    return reader.ReadUInt16();
                }
            }
        }
    }
}

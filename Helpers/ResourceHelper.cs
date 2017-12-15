//
// Authors:
//   刘静谊 (Johnny Liu) <jingeelio@163.com>
//
// Copyright (c) 2017 刘静谊 (Johnny Liu)
//
// Licensed under the LGPLv3 license. Please see <http://www.gnu.org/licenses/lgpl-3.0.html> for license text.
//


using System;
using System.Reflection;
using JointCode.Common.Extensions;

namespace JointCode.Common.Helpers
{
    public class ResourceHelper
    {
        // 资源文件名的组成：默认命名空间.目录.资源文件名（不带后缀）
        // 也可以在资源程序集中定义一个自定义 Attribute，用来携带资源程序集的默认命名空间，然后再通过反射获得此值，这样就可以调用以下方法获得资源名

//#if RUNSHARP
//        const string DefaultNamespace = "JointCode.Expressions";
//#else
//        const string DefaultNamespace = "JointCode";
//#endif
        //internal static string GetBaseResourceName(string directory, string resourceName)
        //{
        //    return DefaultNamespace + "." + directory + "." + resourceName;
        //}

        const string DefaultTextResourcePostfix = ".resources";

        public static string[] GetFullResourceNames(Assembly resourceAssembly)
        { return resourceAssembly.GetManifestResourceNames(); }

        public static string GetFullResourceName(Assembly resourceAssembly, string resourceFileName)
        {
            string result = null;
            var fullResouceNames = resourceAssembly.GetManifestResourceNames();
            
            foreach (var fullResouceName in fullResouceNames)
            {
                if (!fullResouceName.EndsWith(resourceFileName, StringComparison.InvariantCultureIgnoreCase))
                    continue;
                if (result != null)
                    throw new ArgumentException(string.Format(
                        "The specified resource file name [{0}] is not unique! There are other resources with the same file name exists in the same assembly!", resourceFileName));
                result = fullResouceName;
            }

            return result;
        }

        public static string GetBaseResourceName(Assembly resourceAssembly, string resourceDirectory, string resourceName)
        {
            var names = resourceAssembly.GetManifestResourceNames();
            var symbolName = resourceDirectory.IsNullOrWhiteSpace()
                ? resourceName + DefaultTextResourcePostfix
                : resourceDirectory + "." + resourceName + DefaultTextResourcePostfix;

            foreach (var name in names)
            {
                if (name.EndsWith(symbolName))
                    return name.Remove(name.Length - DefaultTextResourcePostfix.Length);
            }

            return null;
        }
    }
}

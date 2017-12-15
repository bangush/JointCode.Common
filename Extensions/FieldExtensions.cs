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

namespace JointCode.Common.Extensions
{
    public static class FieldExtensions
    {
        static readonly Type FieldOnTypeBuilderInstantiation;

        static FieldExtensions()
        {
            FieldOnTypeBuilderInstantiation = Type.GetType("System.Reflection.Emit.FieldOnTypeBuilderInstantiation");
        }

        public static bool IsConstructedGeneric(this FieldInfo field)
        {
            var underlyingType = field.GetType();
            return underlyingType == FieldOnTypeBuilderInstantiation;
        }
    }
}

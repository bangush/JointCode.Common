//
// Authors:
//   刘静谊 (Johnny Liu) <jingeelio@163.com>
//
// Copyright (c) 2017 刘静谊 (Johnny Liu)
//
// Licensed under the LGPLv3 license. Please see <http://www.gnu.org/licenses/lgpl-3.0.html> for license text.
//

using System;
using System.Collections.Generic;
using JointCode.Common.Helpers;

namespace JointCode.Common
{
    public delegate void MyAction();
    public delegate void MyAction<in T>(T arg1);
    public delegate void MyAction<in T1, in T2>(T1 arg1, T2 arg2);
    public delegate void MyAction<in T1, in T2, in T3>(T1 arg1, T2 arg2, T3 arg3);
    public delegate void MyAction<in T1, in T2, in T3, in T4>(T1 arg1, T2 arg2, T3 arg3, T4 arg4);
    public delegate void MyAction<in T1, in T2, in T3, in T4, in T5>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5);
    public delegate void MyAction<in T1, in T2, in T3, in T4, in T5, in T6>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6);
    public delegate void MyAction<in T1, in T2, in T3, in T4, in T5, in T6, in T7>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7);
    public delegate void MyAction<in T1, in T2, in T3, in T4, in T5, in T6, in T7, in T8>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8);
    public delegate void MyAction<in T1, in T2, in T3, in T4, in T5, in T6, in T7, in T8, in T9>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9);
    public delegate void MyAction<in T1, in T2, in T3, in T4, in T5, in T6, in T7, in T8, in T9, in T10>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10);
    public delegate void MyAction<in T1, in T2, in T3, in T4, in T5, in T6, in T7, in T8, in T9, in T10, in T11>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11);
    public delegate void MyAction<in T1, in T2, in T3, in T4, in T5, in T6, in T7, in T8, in T9, in T10, in T11, in T12>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12);
    public delegate void MyAction<in T1, in T2, in T3, in T4, in T5, in T6, in T7, in T8, in T9, in T10, in T11, in T12, in T13>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13);
    public delegate void MyAction<in T1, in T2, in T3, in T4, in T5, in T6, in T7, in T8, in T9, in T10, in T11, in T12, in T13, in T14>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14);
    public delegate void MyAction<in T1, in T2, in T3, in T4, in T5, in T6, in T7, in T8, in T9, in T10, in T11, in T12, in T13, in T14, in T15>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15);
    public delegate void MyAction<in T1, in T2, in T3, in T4, in T5, in T6, in T7, in T8, in T9, in T10, in T11, in T12, in T13, in T14, in T15, in T16>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15, T16 arg16);

    public delegate TResult MyFunc<out TResult>();
    public delegate TResult MyFunc<in T, out TResult>(T arg1);
    public delegate TResult MyFunc<in T1, in T2, out TResult>(T1 arg1, T2 arg2);
    public delegate TResult MyFunc<in T1, in T2, in T3, out TResult>(T1 arg1, T2 arg2, T3 arg3);
    public delegate TResult MyFunc<in T1, in T2, in T3, in T4, out TResult>(T1 arg1, T2 arg2, T3 arg3, T4 arg4);
    public delegate TResult MyFunc<in T1, in T2, in T3, in T4, in T5, out TResult>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5);
    public delegate TResult MyFunc<in T1, in T2, in T3, in T4, in T5, in T6, out TResult>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6);
    public delegate TResult MyFunc<in T1, in T2, in T3, in T4, in T5, in T6, in T7, out TResult>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7);
    public delegate TResult MyFunc<in T1, in T2, in T3, in T4, in T5, in T6, in T7, in T8, out TResult>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8);
    public delegate TResult MyFunc<in T1, in T2, in T3, in T4, in T5, in T6, in T7, in T8, in T9, out TResult>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9);
    public delegate TResult MyFunc<in T1, in T2, in T3, in T4, in T5, in T6, in T7, in T8, in T9, in T10, out TResult>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10);
    public delegate TResult MyFunc<in T1, in T2, in T3, in T4, in T5, in T6, in T7, in T8, in T9, in T10, in T11, out TResult>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11);
    public delegate TResult MyFunc<in T1, in T2, in T3, in T4, in T5, in T6, in T7, in T8, in T9, in T10, in T11, in T12, out TResult>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12);
    public delegate TResult MyFunc<in T1, in T2, in T3, in T4, in T5, in T6, in T7, in T8, in T9, in T10, in T11, in T12, in T13, out TResult>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13);
    public delegate TResult MyFunc<in T1, in T2, in T3, in T4, in T5, in T6, in T7, in T8, in T9, in T10, in T11, in T12, in T13, in T14, out TResult>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14);
    public delegate TResult MyFunc<in T1, in T2, in T3, in T4, in T5, in T6, in T7, in T8, in T9, in T10, in T11, in T12, in T13, in T14, in T15, out TResult>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15);
    public delegate TResult MyFunc<in T1, in T2, in T3, in T4, in T5, in T6, in T7, in T8, in T9, in T10, in T11, in T12, in T13, in T14, in T15, in T16, out TResult>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15, T16 arg16);

    public static class DelegateHelper
    {
        static int _inParamCount = 16;
        static readonly Dictionary<int, Type> _actionTypes = new Dictionary<int, Type>(_inParamCount);
        static readonly Dictionary<int, Type> _funcTypes = new Dictionary<int, Type>(_inParamCount + 1);
        //static readonly Dictionary<int, Type> _predicateTypes = new Dictionary<int, Type>(_inParamCount);

        static DelegateHelper()
        {
            InitializeActionTypes();
            InitializeFuncTypes();
        }

        static void InitializeActionTypes()
        {
            _actionTypes.Add(1, typeof(MyAction<>));
            _actionTypes.Add(2, typeof(MyAction<,>));
            _actionTypes.Add(3, typeof(MyAction<,,>));
            _actionTypes.Add(4, typeof(MyAction<,,,>));
            _actionTypes.Add(5, typeof(MyAction<,,,,>));
            _actionTypes.Add(6, typeof(MyAction<,,,,,>));
            _actionTypes.Add(7, typeof(MyAction<,,,,,,>));
            _actionTypes.Add(8, typeof(MyAction<,,,,,,,>));
            _actionTypes.Add(9, typeof(MyAction<,,,,,,,,>));
            _actionTypes.Add(10, typeof(MyAction<,,,,,,,,,>));
            _actionTypes.Add(11, typeof(MyAction<,,,,,,,,,,>));
            _actionTypes.Add(12, typeof(MyAction<,,,,,,,,,,,>));
            _actionTypes.Add(13, typeof(MyAction<,,,,,,,,,,,,>));
            _actionTypes.Add(14, typeof(MyAction<,,,,,,,,,,,,,>));
            _actionTypes.Add(15, typeof(MyAction<,,,,,,,,,,,,,,>));
            _actionTypes.Add(16, typeof(MyAction<,,,,,,,,,,,,,,,>));
        }

        static void InitializeFuncTypes()
        {
            _funcTypes.Add(1, typeof(MyFunc<>));
            _funcTypes.Add(2, typeof(MyFunc<,>));
            _funcTypes.Add(3, typeof(MyFunc<,,>));
            _funcTypes.Add(4, typeof(MyFunc<,,,>));
            _funcTypes.Add(5, typeof(MyFunc<,,,,>));
            _funcTypes.Add(6, typeof(MyFunc<,,,,,>));
            _funcTypes.Add(7, typeof(MyFunc<,,,,,,>));
            _funcTypes.Add(8, typeof(MyFunc<,,,,,,,>));
            _funcTypes.Add(9, typeof(MyFunc<,,,,,,,,>));
            _funcTypes.Add(10, typeof(MyFunc<,,,,,,,,,>));
            _funcTypes.Add(11, typeof(MyFunc<,,,,,,,,,,>));
            _funcTypes.Add(12, typeof(MyFunc<,,,,,,,,,,,>));
            _funcTypes.Add(13, typeof(MyFunc<,,,,,,,,,,,,>));
            _funcTypes.Add(14, typeof(MyFunc<,,,,,,,,,,,,,>));
            _funcTypes.Add(15, typeof(MyFunc<,,,,,,,,,,,,,,>));
            _funcTypes.Add(16, typeof(MyFunc<,,,,,,,,,,,,,,,>));
            _funcTypes.Add(17, typeof(MyFunc<,,,,,,,,,,,,,,,,>));
        }

        public static string MethodNameInvoke { get { return "Invoke"; }}

        public static Type GetActionType(Type genParamType)
        {
            Requires.Instance.NotNull(genParamType, "genParamType");
            var genDelegateType = _actionTypes[1];
            return genDelegateType.MakeGenericType(genParamType);
        }

        public static Type GetActionType(params Type[] genParamTypes)
        {
            Requires.Instance.True(genParamTypes.Length > 0, "genParamTypes");
            Type genDelegateType;
            if (!_actionTypes.TryGetValue(genParamTypes.Length, out genDelegateType))
                throw ExceptionHelper.HandleAndReturn(new InvalidOperationException("Can not find a generic delegate type with provided number of parameters!"));
            return genDelegateType.MakeGenericType(genParamTypes);
        }

        public static bool TryGetActionType(Type[] genParamTypes, out Type delegateType)
        {
            Requires.Instance.True(genParamTypes.Length > 0, "genParamTypes");
            Type genDelegateType;
            if (!_actionTypes.TryGetValue(genParamTypes.Length, out genDelegateType))
            {
                delegateType = null;
                return false;
            }
            delegateType = genDelegateType.MakeGenericType(genParamTypes);
            return true;
        }

        public static Type GetFuncType(Type genParamType)
        {
            Requires.Instance.NotNull(genParamType, "genParamType");
            var genDelegateType = _funcTypes[1];
            return genDelegateType.MakeGenericType(genParamType);
        }

        public static Type GetFuncType(params Type[] genParamTypes)
        {
            Requires.Instance.True(genParamTypes.Length > 0, "genParamTypes");
            Type genDelegateType;
            if (!_funcTypes.TryGetValue(genParamTypes.Length, out genDelegateType))
                throw ExceptionHelper.HandleAndReturn(new InvalidOperationException("Can not find a generic delegate type with provided number of parameters!"));
            return genDelegateType.MakeGenericType(genParamTypes);
        }

        public static bool TryGetFuncType(Type[] genParamTypes, out Type delegateType)
        {
            Requires.Instance.True(genParamTypes.Length > 0, "genParamTypes");
            Type genDelegateType;
            if (!_funcTypes.TryGetValue(genParamTypes.Length, out genDelegateType))
            {
                delegateType = null;
                return false;
            }
            delegateType = genDelegateType.MakeGenericType(genParamTypes);
            return true;
        }

        //public static Type GetPredicateType(Type genParamType)
        //{
        //    Requires.Instance.NotNull(genParamType, "genParamType");
        //    var genDelegateType = _predicateTypes[1];
        //    return genDelegateType.MakeGenericType(genParamType);
        //}

        //public static Type GetPredicateType(params Type[] genParamTypes)
        //{
        //    Requires.Instance.True(genParamTypes.Length > 0, "genParamTypes");
        //    Type genDelegateType;
        //    if (!_predicateTypes.TryGetValue(genParamTypes.Length, out genDelegateType))
        //        throw _exHandler.HandleAndReturn(new InvalidOperationException("Can not find a generic delegate type with provided number of parameters!"));
        //    return genDelegateType.MakeGenericType(genParamTypes);
        //}

        //public static bool TryGetPredicateType(Type[] genParamTypes, out Type delegateType)
        //{
        //    Requires.Instance.True(genParamTypes.Length > 0, "genParamTypes");
        //    Type genDelegateType;
        //    if (!_predicateTypes.TryGetValue(genParamTypes.Length, out genDelegateType))
        //    {
        //        delegateType = null;
        //        return false;
        //    }
        //    delegateType = genDelegateType.MakeGenericType(genParamTypes);
        //    return true;
        //}
    }
}

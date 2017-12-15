using System;
using System.Collections.Generic;

#if ENCRYPT
namespace JointCode.Internals
#else
namespace JointCode.Common.Conversion
#endif
{
    static class StringToIntConverterRegistrar
    {
        class StringToIntConverter : ObjectConverter<string, int>
        {
            public override int Convert(string input)
            {
                int result;
                return int.TryParse(input, out result) ? result : default(int);
            }

            public override bool TryConvert(string input, out int output)
            {
                return int.TryParse(input, out output);
            }
        }

        static readonly StringToIntConverter _objectConverter = new StringToIntConverter();

        internal static void OnConverterRequested(ConverterRequestedEventArgs args)
        {
            if (args.InputType != typeof(string) || args.OutputType != typeof(int)) return;
            args.ObjectConverter = _objectConverter;
        }
    }

    static class ConverterHelper
    {
        internal static void GetConverterTypes(ObjectConverter objectConverter, out Type inputType, out Type outputType)
        {
            if (objectConverter == null) throw new ArgumentNullException("objectConverter");

            var type = objectConverter.GetType();
            if (!type.IsGenericType || type.GetGenericTypeDefinition() != typeof(ObjectConverter<,>))
                throw new ArgumentException("The converter must be a derivation of Converter<TInput, TOutput>!", "objectConverter");

            var arguments = type.GetGenericArguments();
            inputType = arguments[0];
            outputType = arguments[1];
        }
    }

    /// <summary>
    /// Converts from the input to output.
    /// </summary>
    public abstract class ObjectConverter
    {
        /// <summary>
        /// Converts from the input to output.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns></returns>
        public abstract object Convert(object input);

        /// <summary>
        /// Tries to convert from the input to output.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="output">The output.</param>
        /// <returns></returns>
        public abstract bool TryConvert(object input, out object output);
    }

    /// <summary>
    /// Convert from <see cref="TInput"/> to <see cref="TOutput"/>.
    /// </summary>
    /// <typeparam name="TInput">The input type.</typeparam>
    /// <typeparam name="TOutput">The output type.</typeparam>
    public abstract class ObjectConverter<TInput, TOutput> : ObjectConverter
    {
        public sealed override object Convert(object input) { return Convert((TInput)input); }

        public sealed override bool TryConvert(object input, out object output)
        {
            TOutput result;
            if (TryConvert((TInput)input, out result))
            {
                output = result;
                return true;
            }
            output = null;
            return false;
        }

        /// <summary>
        /// Converts from the input to output.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns></returns>
        public abstract TOutput Convert(TInput input);

        /// <summary>
        /// Tries to convert from the input to output.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="output">The output.</param>
        /// <returns></returns>
        public abstract bool TryConvert(TInput input, out TOutput output);
    }

    #region Generic converters
    /// <summary>
    /// Argument class used by the Converts class when generating the GlobalSearching
    /// and the Searching events. If a Converter is filled, the Searching will stop
    /// (so no other handler will be invoked) and that will be the handler used
    /// as return for the next calls with the same input/output types.
    /// </summary>
    public sealed class ConverterRequestedEventArgs : EventArgs
    {
        internal ConverterRequestedEventArgs(Type inputType, Type outputType)
        {
            InputType = inputType;
            OutputType = outputType;
        }

        /// <summary>
        /// Gets the Input type that the converter should receive.
        /// </summary>
        public Type InputType { get; private set; }

        /// <summary>
        /// Gets the Output type that the converter should return.
        /// </summary>
        public Type OutputType { get; private set; }

        ObjectConverter _objectConverter;
        /// <summary>
        /// Gets or sets the Converter. After setting the converter, no other
        /// handlers will be executed.
        /// The converter should be of type Converter&lt;InputType, OutputType&gt;
        /// </summary>
        public ObjectConverter ObjectConverter
        {
            get
            {
                return _objectConverter;
            }
            set
            {
                Type inputType, outputType;
                ConverterHelper.GetConverterTypes(value, out inputType, out outputType);

                if (!inputType.IsAssignableFrom(InputType))
                    throw new ArgumentException("Converter must have a input type compatible with " + InputType.FullName, "value");
                if (!OutputType.IsAssignableFrom(outputType))
                    throw new ArgumentException("Converter must have a output type compatible with " + OutputType.FullName, "value");

                _objectConverter = value;
            }
        }
    }

    /// <summary>
    /// Class responsible for being the single entry point for all data-type conversions.
    /// You can register static (AppDomain global) and local (per thread) converters here.
    /// </summary>
    public sealed class ConvertionManager
    {
        #region Global

        static readonly Dictionary<KeyValuePair<Type, Type>, ObjectConverter> _staticConverters;

        static ConvertionManager()
        {
            _staticConverters = new Dictionary<KeyValuePair<Type, Type>, ObjectConverter>();
        }

        /// <summary>
        /// Registers a converter at AppDomain level.
        /// </summary>
        public static void RegisterStatic<TInput, TOutput>(ObjectConverter<TInput, TOutput> objectConverter)
        {
            if (objectConverter == null) throw new ArgumentNullException("objectConverter");
            var key = new KeyValuePair<Type, Type>(typeof(TInput), typeof(TOutput));
            lock (_staticConverters)
                _staticConverters[key] = objectConverter;
        }

        /// <summary>
        /// Registers a converter at AppDomain level.
        /// </summary>
        public static void RegisterStatic(ObjectConverter objectConverter)
        {
            Type inputType, outputType;
            ConverterHelper.GetConverterTypes(objectConverter, out inputType, out outputType);
            var key = new KeyValuePair<Type, Type>(inputType, outputType);
            lock (_staticConverters)
                _staticConverters[key] = objectConverter;
        }

        /// <summary>
        /// Tries to get a converter for the given input/output types from the static converter cache.
        /// </summary>
        public static ObjectConverter<TInput, TOutput> TryGetStatic<TInput, TOutput>()
        {
            var result = TryGetStatic(typeof(TInput), typeof(TOutput));
            var typedResult = (ObjectConverter<TInput, TOutput>)(result);
            return typedResult;
        }

        /// <summary>
        /// Tries to get a converter for the given input/output types from the static converter cache. 
        /// Throws when no converters registered for the given types.
        /// </summary>
        public static ObjectConverter<TInput, TOutput> GetStatic<TInput, TOutput>()
        {
            var result = TryGetStatic<TInput, TOutput>();
            if (result == null) throw new InvalidOperationException("There is no converter registered for the given types.");
            return result;
        }

        /// <summary>
        /// Tries to get a converter for the given input/output types from the static converter cache.
        /// </summary>
        public static ObjectConverter TryGetStatic(Type inputType, Type outputType)
        {
            if (inputType == null) throw new ArgumentNullException("inputType");
            if (outputType == null) throw new ArgumentNullException("outputType");
            var key = new KeyValuePair<Type, Type>(inputType, outputType);
            return _TryGetConverterStatic(ref key);
        }

        static ObjectConverter _TryGetConverterStatic(ref KeyValuePair<Type, Type> key)
        {
            ObjectConverter result;
            Action<ConverterRequestedEventArgs> converterRequested;
            lock (_staticConverters)
            {
                if (_staticConverters.TryGetValue(key, out result)) return result;
                converterRequested = StaticConverterRequested;
            }

            if (converterRequested != null)
            {
                var args = new ConverterRequestedEventArgs(key.Key, key.Value);
                var handlers = converterRequested.GetInvocationList();

                Action<ConverterRequestedEventArgs> matchingHandler = null;
                for (int i = 0; i < handlers.Length; i++)
                {
                    var handler = (Action<ConverterRequestedEventArgs>)handlers[i];
                    handler(args);
                    result = args.ObjectConverter;
                    if (result != null)
                    {
                        matchingHandler = handler;
                        break;
                    }
                }

                if (matchingHandler != null)
                {
                    lock (_staticConverters)
                    {
                        StaticConverterRequested -= matchingHandler;
                        _staticConverters[key] = result;
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Occurs when a converter is requested but no converter is found.
        /// </summary>
        public static event Action<ConverterRequestedEventArgs> StaticConverterRequested;

        #endregion

        #region Local

        readonly Dictionary<KeyValuePair<Type, Type>, ObjectConverter> _converters = new Dictionary<KeyValuePair<Type, Type>, ObjectConverter>();

        /// <summary>
        /// Registers a converter for the given input/output types for the actual thread.
        /// If you want to register globally, use the static GlobalRegister method.
        /// </summary>
        public void Register<TInput, TOutput>(ObjectConverter<TInput, TOutput> objectConverter)
        {
            if (objectConverter == null) throw new ArgumentNullException("objectConverter");
            var key = new KeyValuePair<Type, Type>(typeof(TInput), typeof(TOutput));
            _converters[key] = objectConverter;
        }

        public void Register(ObjectConverter objectConverter)
        {
            Type inputType, outputType;
            ConverterHelper.GetConverterTypes(objectConverter, out inputType, out outputType);
            var key = new KeyValuePair<Type, Type>(inputType, outputType);
            _converters[key] = objectConverter;
        }

        /// <summary>
        /// Tries to get a converter for the given input/output types.
        /// It will first look for thread specific ones and, if none is found, will look
        /// for global ones. This method will return null if none is found.
        /// </summary>
        public ObjectConverter<TInput, TOutput> TryGet<TInput, TOutput>()
        {
            var result = TryGet(typeof(TInput), typeof(TOutput));
            var typedResult = (ObjectConverter<TInput, TOutput>)(result);
            return typedResult;
        }

        /// <summary>
        /// Tries to get a converter for the given input/output types.
        /// It will first look for thread specific ones and, if none is found, will look
        /// for global ones. This method throws an InvalidOperationException if none is found.
        /// </summary>
        public ObjectConverter<TInput, TOutput> Get<TInput, TOutput>()
        {
            var result = TryGet<TInput, TOutput>();
            if (result == null) throw new InvalidOperationException("There is no converter registered for the given types.");
            return result;
        }

        /// <summary>
        /// Tries to get the delegate to convert from the given input-type to the
        /// given output type. This method returns the delegate untyped, but
        /// it is a valid Converter&lt;inputType, outputType&gt; delegate.
        /// If you need to invoke it with the parameters cast as object, use
        /// the CastedTryGet method.
        /// </summary>
        public ObjectConverter TryGet(Type inputType, Type outputType)
        {
            if (inputType == null) throw new ArgumentNullException("inputType");
            if (outputType == null) throw new ArgumentNullException("outputType");
            var key = new KeyValuePair<Type, Type>(inputType, outputType);
            return _TryGet(ref key) ?? _TryGetConverterStatic(ref key);
        }

        ObjectConverter _TryGet(ref KeyValuePair<Type, Type> key)
        {
            ObjectConverter result;
            if (_converters.TryGetValue(key, out result)) return result;

            Action<ConverterRequestedEventArgs> converterRequested;
            lock (_converters) converterRequested = ConverterRequested;

            if (converterRequested != null)
            {
                var args = new ConverterRequestedEventArgs(key.Key, key.Value);
                var handlers = converterRequested.GetInvocationList();

                Action<ConverterRequestedEventArgs> matchingHandler = null;
                for (int i = 0; i < handlers.Length; i++)
                {
                    var handler = (Action<ConverterRequestedEventArgs>)handlers[i];
                    handler(args);
                    result = args.ObjectConverter;
                    if (result != null)
                    {
                        matchingHandler = handler;
                        break;
                    }
                }

                if (matchingHandler != null)
                {
                    lock (_converters) ConverterRequested -= matchingHandler;
                    _converters[key] = result;
                }
            }

            return result;
        }

        /// <summary>
        /// Event invoked when a converter for an specific input/output type is not found for
        /// the actual thread. Note that global converters will only be used if the Searching
        /// event does not return a converter.
        /// </summary>
        public event Action<ConverterRequestedEventArgs> ConverterRequested;

        #endregion
    } 
    #endregion

    #region Specific converters
    //public sealed class ConverterRequestedEventArgs<TInput> : EventArgs
    //{
    //    ObjectConverter<TInput, object> _objectConverter;

    //    internal ConverterRequestedEventArgs(Type outputType)
    //    {
    //        OutputType = outputType;
    //    }

    //    /// <summary>
    //    /// Gets the Input type that the converter should receive.
    //    /// </summary>
    //    public Type InputType { get { return typeof(TInput); } }

    //    /// <summary>
    //    /// Gets the Output type that the converter should return.
    //    /// </summary>
    //    public Type OutputType { get; private set; }

    //    /// <summary>
    //    /// Gets or sets the Converter. After setting the converter, no other
    //    /// handlers will be executed.
    //    /// The converter should be of type Converter&lt;InputType, OutputType&gt;
    //    /// </summary>
    //    public ObjectConverter<TInput, object> ObjectConverter
    //    {
    //        get
    //        {
    //            return _objectConverter;
    //        }
    //        set
    //        {
    //            Type inputType, outputType;
    //            ConverterHelper.GetConverterTypes(value, out inputType, out outputType);

    //            if (!inputType.IsAssignableFrom(InputType))
    //                throw new ArgumentException("Converter must have a input type compatible with " + InputType.FullName, "value");
    //            if (!OutputType.IsAssignableFrom(outputType))
    //                throw new ArgumentException("Converter must have a output type compatible with " + OutputType.FullName, "value");

    //            _objectConverter = value;
    //        }
    //    }
    //}

    //public sealed class ConvertionManager<TInput>
    //{
    //    #region Local

    //    readonly Dictionary<Type, ObjectConverter<TInput, object>> _converters
    //        = new Dictionary<Type, ObjectConverter<TInput, object>>();

    //    /// <summary>
    //    /// Registers a converter for the given input/output types for the actual thread.
    //    /// If you want to register globally, use the static GlobalRegister method.
    //    /// </summary>
    //    public void Register(ObjectConverter<TInput, object> objectConverter)
    //    {
    //        if (objectConverter == null) throw new ArgumentNullException("objectConverter");
    //        var key = new KeyValuePair<Type, Type>(typeof(TInput), typeof(object));
    //        _converters[key] = objectConverter;
    //    }

    //    /// <summary>
    //    /// Tries to get a converter for the given input/output types.
    //    /// It will first look for thread specific ones and, if none is found, will look
    //    /// for global ones. This method will return null if none is found.
    //    /// </summary>
    //    public ObjectConverter<TInput, object> TryGet()
    //    {
    //        var key = new KeyValuePair<Type, Type>(typeof(TInput), typeof(object));
    //        return _TryGet(ref key);
    //    }

    //    /// <summary>
    //    /// Tries to get a converter for the given input/output types.
    //    /// It will first look for thread specific ones and, if none is found, will look
    //    /// for global ones. This method throws an InvalidOperationException if none is found.
    //    /// </summary>
    //    public ObjectConverter<TInput, object> Get()
    //    {
    //        var result = TryGet();
    //        if (result == null) throw new InvalidOperationException("There is no converter registered for the given types.");
    //        return result;
    //    }

    //    ObjectConverter<TInput, object> _TryGet(ref KeyValuePair<Type, Type> key)
    //    {
    //        ObjectConverter<TInput, object> result;
    //        if (_converters.TryGetValue(key, out result)) return result;

    //        Action<ConverterRequestedEventArgs<TInput>> converterRequested;
    //        lock (_converters) converterRequested = ConverterRequested;

    //        if (converterRequested != null)
    //        {
    //            var args = new ConverterRequestedEventArgs<TInput>(key.Key, key.Value);
    //            var handlers = converterRequested.GetInvocationList();

    //            Action<ConverterRequestedEventArgs> matchingHandler = null;
    //            for (int i = 0; i < handlers.Length; i++)
    //            {
    //                var handler = (Action<ConverterRequestedEventArgs<TInput>>)handlers[i];
    //                handler(args);
    //                result = args.ObjectConverter;
    //                if (result != null)
    //                {
    //                    matchingHandler = handler;
    //                    break;
    //                }
    //            }

    //            if (matchingHandler != null)
    //            {
    //                lock (_converters) ConverterRequested -= matchingHandler;
    //                _converters[key] = result;
    //            }
    //        }

    //        return result;
    //    }

    //    /// <summary>
    //    /// Event invoked when a converter for an specific input/output type is not found for
    //    /// the actual thread. Note that global converters will only be used if the Searching
    //    /// event does not return a converter.
    //    /// </summary>
    //    public event Action<ConverterRequestedEventArgs<TInput>> ConverterRequested;

    //    #endregion
    //} 
    #endregion
}

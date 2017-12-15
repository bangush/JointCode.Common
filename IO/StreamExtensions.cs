//#define NO_UNSAFE
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
using System.IO;
using System.Runtime.InteropServices;
using System.Security;

#if ENCRYPT
namespace JointCode.Internals
#else

using JointCode.Common.Extensions;
using JointCode.Common.Helpers;

namespace JointCode.Common.IO
#endif
{
    /// 关于 StreamExtensions 的 Todo 列表
    /// ===============================================================================
    /// 1. Skip* 方法要实现
    /// 2. WriteString/ReadString 方法中，读写字符串长度时，要用 uint，而不要用 int
    public static partial class StreamExtensions
    {
        [ThreadStatic]
        static BinaryHelper _helper;

        //[SecuritySafeCritical]
        //public static byte ReadByte(this Stream stream)
        //{
        //    return (byte)stream.ReadByte();
        //}
        //[SecuritySafeCritical]
        //public static void WriteByte(this Stream stream, byte value)
        //{
        //    stream.WriteByte(value);
        //}

        [SecuritySafeCritical]
        public static sbyte ReadSByte(this Stream stream)
        {
            return (sbyte)stream.ReadByte();
        }
        [SecuritySafeCritical]
        public static void WriteSByte(this Stream stream, sbyte value)
        {
            stream.WriteByte((byte)value);
        }

        [SecuritySafeCritical]
        static byte[] ReadBytes(Stream stream, int length)
        {
            var bytes = new byte[length];
            stream.Read(bytes, 0, length);
            return bytes;
        }

        [SecuritySafeCritical]
        public static byte[] ReadBytes(this Stream stream)
        {
            int totalBytes = ReadInt32(stream);

            if (totalBytes == 0)
                return null;
            if (totalBytes == 1)
                return BinaryHelper.EmptyByteBuffer;

            totalBytes -= 1;

            return ReadBytes(stream, totalBytes);
        }
        [SecuritySafeCritical]
        public static void WriteBytes(this Stream stream, byte[] value)
        {
            if (value == null)
            {
                WriteInt32(stream, 0);
                return;
            }
            WriteInt32(stream, value.Length + 1);
            stream.Write(value, 0, value.Length);
        }

        [SecuritySafeCritical]
        public static short ReadInt16(this Stream stream)
        {
            var b1 = stream.ReadByte();
            var b2 = stream.ReadByte();
            return (short)(b1 | (b2 << 8));
        }
        [SecuritySafeCritical]
        public static void WriteInt16(this Stream stream, short value)
        {
            var b1 = (byte)value;
            var b2 = (byte)(value >> 8);
            stream.WriteByte(b1);
            stream.WriteByte(b2);
        }

        [SecuritySafeCritical]
        public static ushort ReadUInt16(this Stream stream)
        {
            var b1 = stream.ReadByte();
            var b2 = stream.ReadByte();
            return (ushort)(b1 | (b2 << 8));
        }
        [SecuritySafeCritical]
        public static void WriteUInt16(this Stream stream, ushort value)
        {
            var b1 = (byte)value;
            var b2 = (byte)(value >> 8);
            stream.WriteByte(b1);
            stream.WriteByte(b2);
        }

        [SecuritySafeCritical]
        public static int ReadInt32(this Stream stream)
        {
            var b1 = stream.ReadByte();
            var b2 = stream.ReadByte();
            var b3 = stream.ReadByte();
            var b4 = stream.ReadByte();
            return b1 | (b2 << 8) | (b3 << 16) | (b4 << 24);
        }
        [SecuritySafeCritical]
        public static void WriteInt32(this Stream stream, int value)
        {
            var b1 = (byte)(value);
            var b2 = (byte)(value >> 8);
            var b3 = (byte)(value >> 16);
            var b4 = (byte)(value >> 24);
            stream.WriteByte(b1);
            stream.WriteByte(b2);
            stream.WriteByte(b3);
            stream.WriteByte(b4);
        }

        [SecuritySafeCritical]
        public static uint ReadUInt32(this Stream stream)
        {
            var b1 = stream.ReadByte();
            var b2 = stream.ReadByte();
            var b3 = stream.ReadByte();
            var b4 = stream.ReadByte();
            return (uint)(b1 | (b2 << 8) | (b3 << 16) | (b4 << 24));
        }
        [SecuritySafeCritical]
        public static void WriteUInt32(this Stream stream, uint value)
        {
            var b1 = (byte)(value);
            var b2 = (byte)(value >> 8);
            var b3 = (byte)(value >> 16);
            var b4 = (byte)(value >> 24);
            stream.WriteByte(b1);
            stream.WriteByte(b2);
            stream.WriteByte(b3);
            stream.WriteByte(b4);
        }

        [SecuritySafeCritical]
        public static long ReadInt64(this Stream stream)
        {
            var b1 = (long)stream.ReadByte();
            var b2 = (long)stream.ReadByte();
            var b3 = (long)stream.ReadByte();
            var b4 = (long)stream.ReadByte();
            var b5 = (long)stream.ReadByte();
            var b6 = (long)stream.ReadByte();
            var b7 = (long)stream.ReadByte();
            var b8 = (long)stream.ReadByte();
            var value = 0L;
            value |= b1;
            value |= (b2 << 8);
            value |= (b3 << 16);
            value |= (b4 << 24);
            value |= (b5 << 32);
            value |= (b6 << 40);
            value |= (b7 << 48);
            value |= (b8 << 56);
            return value;
        }
        [SecuritySafeCritical]
        public static void WriteInt64(this Stream stream, long value)
        {
            var b1 = (byte)value;
            var b2 = (byte)(value >> 8);
            var b3 = (byte)(value >> 16);
            var b4 = (byte)(value >> 24);
            var b5 = (byte)(value >> 32);
            var b6 = (byte)(value >> 40);
            var b7 = (byte)(value >> 48);
            var b8 = (byte)(value >> 56);
            stream.WriteByte(b1);
            stream.WriteByte(b2);
            stream.WriteByte(b3);
            stream.WriteByte(b4);
            stream.WriteByte(b5);
            stream.WriteByte(b6);
            stream.WriteByte(b7);
            stream.WriteByte(b8);
        }

        [SecuritySafeCritical]
        public static ulong ReadUInt64(this Stream stream)
        {
            var b1 = (long)stream.ReadByte();
            var b2 = (long)stream.ReadByte();
            var b3 = (long)stream.ReadByte();
            var b4 = (long)stream.ReadByte();
            var b5 = (long)stream.ReadByte();
            var b6 = (long)stream.ReadByte();
            var b7 = (long)stream.ReadByte();
            var b8 = (long)stream.ReadByte();
            var value = 0L;
            value |= b1;
            value |= (b2 << 8);
            value |= (b3 << 16);
            value |= (b4 << 24);
            value |= (b5 << 32);
            value |= (b6 << 40);
            value |= (b7 << 48);
            value |= (b8 << 56);
            return (ulong)value;
        }
        [SecuritySafeCritical]
        public static void WriteUInt64(this Stream stream, ulong value)
        {
            var b1 = (byte)value;
            var b2 = (byte)(value >> 8);
            var b3 = (byte)(value >> 16);
            var b4 = (byte)(value >> 24);
            var b5 = (byte)(value >> 32);
            var b6 = (byte)(value >> 40);
            var b7 = (byte)(value >> 48);
            var b8 = (byte)(value >> 56);
            stream.WriteByte(b1);
            stream.WriteByte(b2);
            stream.WriteByte(b3);
            stream.WriteByte(b4);
            stream.WriteByte(b5);
            stream.WriteByte(b6);
            stream.WriteByte(b7);
            stream.WriteByte(b8);
        }

        [SecuritySafeCritical]
        public static bool ReadBoolean(this Stream stream)
        {
            var b = stream.ReadByte();
            if (b == 0) return false;
            if (b == 1) return true;
            throw new IOException("Data broken!");
        }
        [SecuritySafeCritical]
        public static void WriteBoolean(this Stream stream, bool value)
        {
            if (value) stream.WriteByte(1);
            else stream.WriteByte(0);
        }

        [SecuritySafeCritical]
        public static char ReadChar(this Stream stream)
        {
            var b1 = stream.ReadByte();
            var b2 = stream.ReadByte();
            var value = 0;
            value |= b1;
            value |= (b2 << 8);
            return (char)value;
        }
        [SecuritySafeCritical]
        public static void WriteChar(this Stream stream, char value)
        {
            var b1 = (byte)(value);
            var b2 = (byte)(value >> 8);
            stream.WriteByte(b1);
            stream.WriteByte(b2);
        }

        [SecuritySafeCritical]
        public static DateTime ReadDateTime(this Stream stream)
        {
            long value = ReadInt64(stream);
#if MONO
            return new DateTime(value);
#else
            return DateTime.FromBinary(value);
#endif
        }
        [SecuritySafeCritical]
        public static void WriteDateTime(this Stream stream, DateTime value)
        {
#if MONO
            long binary = value.ToFileTime();
#else
            long binary = value.ToBinary();
#endif
            WriteInt64(stream, binary);
        }

        [SecuritySafeCritical]
        public static decimal ReadDecimal(this Stream stream)
        {
            var newDecimal = new int[4];
            newDecimal[0] = ReadInt32(stream);
            newDecimal[1] = ReadInt32(stream);
            newDecimal[2] = ReadInt32(stream);
            newDecimal[3] = ReadInt32(stream);
            return new decimal(newDecimal);
        }
        [SecuritySafeCritical]
        public static void WriteDecimal(this Stream stream, decimal value)
        {
            int[] bits = decimal.GetBits(value);
            WriteInt32(stream, bits[0]);
            WriteInt32(stream, bits[1]);
            WriteInt32(stream, bits[2]);
            WriteInt32(stream, bits[3]);
        }

#if NO_UNSAFE
        [SecuritySafeCritical]
        public static float ReadSingle(this Stream stream)
        {
            var v = ReadInt64(stream);
            // 强制将 double 转换为 float
            return (float)BitConverter.Int64BitsToDouble(v);
        }
        [SecuritySafeCritical]
        public static void WriteSingle(this Stream stream, float value)
        {
            // 隐式将 float 转换为 double
            var v = BitConverter.DoubleToInt64Bits(value);
            WriteInt64(stream, v);
        }

        [SecuritySafeCritical]
        public static double ReadDouble(this Stream stream)
        {
            var v = ReadInt64(stream);
            return BitConverter.Int64BitsToDouble(v);
        }
        [SecuritySafeCritical]
        public static void WriteDouble(this Stream stream, double value)
        {
            var v = BitConverter.DoubleToInt64Bits(value);
            WriteInt64(stream, v);
        }

        [SecuritySafeCritical]
        public static string ReadString(this Stream stream)
        {
            int totalBytes = ReadInt32(stream);

            if (totalBytes == 0)
                return null;
            if (totalBytes == 1)
                return string.Empty;

            totalBytes -= 1;

            var helper = _helper;
            if (helper == null)
                _helper = helper = new BinaryHelper();

            if (totalBytes > BinaryHelper.ByteBufferSize)
            {
                var bytes = ReadBytes(stream, totalBytes);
                return helper.Encoding.GetString(bytes, 0, totalBytes);
            }

            stream.Read(helper.Stream, 0, totalBytes);
            return helper.Encoding.GetString(helper.Stream, 0, totalBytes);
        }
        [SecuritySafeCritical]
        public static void WriteString(this Stream stream, string value)
        {
            if (value == null)
            {
                // write 0 to indicate null.
                WriteInt32(stream, 0);
                return;
            }
            if (value == string.Empty)
            {
                // write 1 to indicate string.Empty.
                WriteInt32(stream, 0);
                return;
            }

            var helper = _helper;
            if (helper == null)
                _helper = helper = new BinaryHelper();

            int totalBytes = helper.Encoding.GetByteCount(value);
            // Write the size of the string
            WriteInt32(stream, totalBytes + 1);

            if (totalBytes > BinaryHelper.ByteBufferSize)
            {
                var bytes = new byte[totalBytes];
                helper.Encoding.GetBytes(value, 0, value.Length, bytes, 0);
                stream.Write(bytes, 0, totalBytes);
            }
            else
            {
                helper.Encoding.GetBytes(value, 0, value.Length, helper.Stream, 0);
                stream.Write(helper.Stream, 0, totalBytes);
            }
        }
#else
        [SecuritySafeCritical]
        public static float ReadSingle(this Stream stream)
        {
            return BinaryHelper.ToSingle(stream.ReadInt32());
        }
        [SecuritySafeCritical]
        public static void WriteSingle(this Stream stream, float value)
        {
            stream.WriteInt32(BinaryHelper.ToInt(value));
        }

        [SecuritySafeCritical]
        public static double ReadDouble(this Stream stream)
        {
            return BinaryHelper.ToDouble(stream.ReadInt64());
        }
        [SecuritySafeCritical]
        public static void WriteDouble(this Stream stream, double value)
        {
            stream.WriteInt64(BinaryHelper.ToLong(value));
        }

        [SecuritySafeCritical]
        public static string ReadString(this Stream stream)
        {
            int totalBytes = ReadInt32(stream);
            if (totalBytes == 0)
                return null;
            if (totalBytes == 1)
                return string.Empty;
            totalBytes -= 1;

            int totalChars = ReadInt32(stream);

            var helper = _helper;
            if (helper == null)
                _helper = helper = new BinaryHelper();

            var decoder = helper.Decoder;
            var buf = helper.Stream;
            char[] chars;
            if (totalChars <= BinaryHelper.CharBufferSize)
                chars = helper.CharBuffer;
            else
                chars = new char[totalChars];

            int streamBytesLeft = totalBytes;
            int cp = 0;

            while (streamBytesLeft > 0)
            {
                int bytesInBuffer = stream.Read(buf, 0, Math.Min(buf.Length, streamBytesLeft));
                if (bytesInBuffer == 0)
                    throw new EndOfStreamException();

                streamBytesLeft -= bytesInBuffer;
                bool flush = streamBytesLeft == 0;
                bool completed = false;
                int p = 0;

                while (completed == false)
                {
                    int charsConverted;
                    int bytesConverted;

                    decoder.Convert(buf, p, bytesInBuffer - p,
                        chars, cp, totalChars - cp,
                        flush,
                        out bytesConverted, out charsConverted, out completed);

                    p += bytesConverted;
                    cp += charsConverted;
                }
            }

            return new string(chars, 0, totalChars);
        }
        [SecuritySafeCritical]
        public static unsafe void WriteString(this Stream stream, string value)
        {
            if (value == null)
            {
                WriteInt32(stream, 0);
                return;
            }
            if (value.Length == 0)
            {
                WriteInt32(stream, 1);
                return;
            }

            var helper = _helper;
            if (helper == null)
                _helper = helper = new BinaryHelper();

            var encoder = helper.Encoder;
            var buf = helper.Stream;

            int totalChars = value.Length;
            int totalBytes;

            fixed (char* ptr = value)
                totalBytes = encoder.GetByteCount(ptr, totalChars, true);

            WriteInt32(stream, totalBytes + 1);
            WriteInt32(stream, totalChars);

            int p = 0;
            bool completed = false;

            while (completed == false)
            {
                int charsConverted;
                int bytesConverted;
                fixed (char* src = value)
                {
                    fixed (byte* dst = buf)
                    {
                        encoder.Convert(src + p, totalChars - p, dst, buf.Length, true,
                            out charsConverted, out bytesConverted, out completed);
                    }
                }
                stream.Write(buf, 0, bytesConverted);
                p += charsConverted;
            }
        }
#endif

        /// <summary>
        /// Reads a TimeSpan from the buffer.
        /// </summary>
        public static TimeSpan ReadTimeSpan(this Stream stream)
        {
            var value = ReadInt64(stream);
            return new TimeSpan(value);
        }
        /// <summary>
        /// Writes a TimeSpan to the buffer.
        /// </summary>
        public static void WriteTimeSpan(this Stream stream, TimeSpan value)
        {
            WriteInt64(stream, value.Ticks);
        }

        /// <summary>
        /// Reads a Guid from the buffer.
        /// </summary>
        public static Guid ReadGuid(this Stream stream)
        {
            var bytes = ReadBytes(stream);
            return new Guid(bytes);
        }
        /// <summary>
        /// Writes a Guid to the buffer.
        /// </summary>
        public static void WriteGuid(this Stream stream, Guid value)
        {
            WriteBytes(stream, value.ToByteArray());
        }

        public static Version ReadVersion(this Stream stream)
        {
            int major = ReadInt32(stream);
            int minor = ReadInt32(stream);
            int build = ReadInt32(stream);
            int revision = ReadInt32(stream);
            if (major == -1 || minor == -1)
                return null;

            Version ver;
            if (build == -1)
                ver = new Version(major, minor);
            else
                ver = revision == -1
                    ? new Version(major, minor, build)
                    : new Version(major, minor, build, revision);
            return ver;
        }
        public static void WriteVersion(this Stream stream, Version value)
        {
            if (value == null || value.Major < 0 || value.Minor < 0)
            {
                WriteInt32(stream, -1);
                WriteInt32(stream, -1);
                WriteInt32(stream, -1);
                WriteInt32(stream, -1);
            }
            else
            {
                WriteInt32(stream, value.Major);
                WriteInt32(stream, value.Minor);
                if (value.Build < 0)
                {
                    WriteInt32(stream, -1);
                    WriteInt32(stream, -1);
                }
                else
                {
                    WriteInt32(stream, value.Build);
                    if (value.Revision < 0)
                        WriteInt32(stream, -1);
                    else
                        WriteInt32(stream, value.Revision);
                }
            }
        }
    }

    // skippers
    partial class StreamExtensions
    {
        [SecuritySafeCritical]
        public static void SkipBoolean(this Stream stream)
        {
            stream.Position += sizeof(byte);
        }

        [SecuritySafeCritical]
        public static void SkipByte(this Stream stream)
        {
            stream.Position += sizeof(byte);
        }

        [SecuritySafeCritical]
        public static void SkipSByte(this Stream stream)
        {
            stream.Position += sizeof(byte);
        }

        [SecuritySafeCritical]
        public static void SkipInt16(this Stream stream)
        {
            stream.Position += sizeof(short);
        }

        [SecuritySafeCritical]
        public static void SkipUInt16(this Stream stream)
        {
            stream.Position += sizeof(short);
        }

        [SecuritySafeCritical]
        public static void SkipChar(this Stream stream)
        {
            stream.Position += sizeof(short);
        }

        [SecuritySafeCritical]
        public static void SkipInt32(this Stream stream)
        {
            stream.Position += sizeof(int);
        }

        [SecuritySafeCritical]
        public static void SkipUInt32(this Stream stream)
        {
            stream.Position += sizeof(int);
        }

        [SecuritySafeCritical]
        public static void SkipInt64(this Stream stream)
        {
            stream.Position += sizeof(long);
        }

        [SecuritySafeCritical]
        public static void SkipUInt64(this Stream stream)
        {
            stream.Position += sizeof(long);
        }

        [SecuritySafeCritical]
        public static void SkipSingle(this Stream stream)
        {
            stream.Position += sizeof(int);
        }

        [SecuritySafeCritical]
        public static void SkipDouble(this Stream stream)
        {
            stream.Position += sizeof(long);
        }

        [SecuritySafeCritical]
        public static void SkipDecimal(this Stream stream)
        {
            stream.Position += sizeof(byte);
        }

        [SecuritySafeCritical]
        public static void SkipString(this Stream stream)
        {
            var strLength = stream.ReadInt32();
            stream.Position += strLength * sizeof(char);
        }

        [SecuritySafeCritical]
        public static void SkipDateTime(this Stream stream)
        {
            stream.Position += sizeof(long);
        }
    }

    public interface ISerializableType
    {
        void Write(Stream stream);
        object Read(Stream stream);
    }

    // read / write default known objects or ISerializableType
    partial class StreamExtensions
    {
        //class ObjectCreator{ internal }
        //static readonly Dictionary<Type, Int16> _type2Code = new Dictionary<Type, short>();

        public static object ReadKnownObject(this Stream stream)
        {
            var c = (KnownTypeCode)stream.ReadByte();
            switch (c)
            {
                case KnownTypeCode.Boolean:
                    return stream.ReadBoolean();

                case KnownTypeCode.Char:
                    return stream.ReadChar();

                case KnownTypeCode.SByte:
                    return stream.ReadSByte();

                case KnownTypeCode.Byte:
                    return stream.ReadByte();

                case KnownTypeCode.Int16:
                    return stream.ReadInt16();

                case KnownTypeCode.UInt16:
                    return stream.ReadUInt16();

                case KnownTypeCode.Int32:
                    return stream.ReadInt32();

                case KnownTypeCode.UInt32:
                    return stream.ReadUInt32();

                case KnownTypeCode.Int64:
                    return stream.ReadInt64();

                case KnownTypeCode.UInt64:
                    return stream.ReadUInt64();

                case KnownTypeCode.Single:
                    return stream.ReadSingle();

                case KnownTypeCode.Double:
                    return stream.ReadDouble();

                case KnownTypeCode.DateTime:
                    return stream.ReadDateTime();

                case KnownTypeCode.String:
                    return stream.ReadString();

                case KnownTypeCode.Version:
                    return stream.ReadVersion();

                case KnownTypeCode.Guid:
                    return stream.ReadGuid();

                case KnownTypeCode.TimeSpan:
                    return stream.ReadTimeSpan();

                default:
                    ExceptionHelper.Handle(new InvalidOperationException("Unexpected value type!"));
                    return null;
            }
        }

        public static void WriteKnownObject(this Stream stream, object obj)
        {
            Requires.Instance.NotNull(obj, "obj");
            var type = obj.GetType();
            var c = Type.GetTypeCode(type);

            switch (c)
            {
                case TypeCode.Boolean: 
                    stream.WriteByte((byte)c); 
                    stream.WriteBoolean((bool)obj);
                    break;

                case TypeCode.Char: 
                    stream.WriteByte((byte)c); 
                    stream.WriteChar((char)obj); 
                    break;

                case TypeCode.SByte: 
                    stream.WriteByte((byte)c); 
                    stream.WriteSByte((sbyte)obj); 
                    break;

                case TypeCode.Byte:
                    stream.WriteByte((byte)c); 
                    stream.WriteByte((byte)obj); 
                    break;

                case TypeCode.Int16: 
                    stream.WriteByte((byte)c); 
                    stream.WriteInt16((short)obj); 
                    break;

                case TypeCode.UInt16: 
                    stream.WriteByte((byte)c); 
                    stream.WriteUInt16((ushort)obj); 
                    break;

                case TypeCode.Int32: 
                    stream.WriteByte((byte)c);
                    stream.WriteInt32((int)obj); 
                    break;

                case TypeCode.UInt32: 
                    stream.WriteByte((byte)c);
                    stream.WriteUInt32((uint)obj); 
                    break;

                case TypeCode.Int64: 
                    stream.WriteByte((byte)c);
                    stream.WriteInt64((long)obj); 
                    break;

                case TypeCode.UInt64: 
                    stream.WriteByte((byte)c);
                    stream.WriteUInt64((ulong)obj); 
                    break;

                case TypeCode.Single: 
                    stream.WriteByte((byte)c);
                    stream.WriteSingle((float)obj); 
                    break;

                case TypeCode.Double: 
                    stream.WriteByte((byte)c);
                    stream.WriteDouble((double)obj); 
                    break;

                case TypeCode.DateTime: 
                    stream.WriteByte((byte)c);
                    stream.WriteDateTime((DateTime)obj); 
                    break;

                case TypeCode.String: 
                    stream.WriteByte((byte)c);
                    stream.WriteString((string)obj); 
                    break;

                default:
                    if (type == typeof(Version))
                    {
                        stream.WriteByte((byte)KnownTypeCode.Version);
                        stream.WriteVersion((Version)obj); 
                    }
                    else if (type == typeof(Guid))
                    {
                        stream.WriteByte((byte)KnownTypeCode.Guid);
                        stream.WriteGuid((Guid)obj);
                    }
                    else if (type == typeof(TimeSpan))
                    {
                        stream.WriteByte((byte)KnownTypeCode.TimeSpan);
                        stream.WriteTimeSpan((TimeSpan)obj);
                    }
                    else
                    {
                        ExceptionHelper.Handle(new InvalidOperationException("Unexpected value type: " + type.ToFullTypeName()));
                    }
                    break;
            }
        }

        public static object[] ReadKnownObjectArray(this Stream stream)
        {
            var length = stream.ReadInt32();
            if (length == -1)
                return null;

            var result = new object[length];
            for (int i = 0; i < length; i++)
            {
                var obj = ReadKnownObject(stream);
                result[i] = obj;
            }

            return result;
        }

        public static void WriteKnownObjectArray(this Stream stream, object[] objs)
        {
            if (objs == null)
            {
                stream.WriteInt32(-1);
            }
            else
            {
                stream.WriteInt32(objs.Length);
                for (int i = 0; i < objs.Length; i++)
                    WriteKnownObject(stream, objs[i]);
            }
        }

        //public static void RegisterKnownType<T>(ref MyFunc<T> objCreator) where T : ISerializableType
        //{
        //}
    }

    public interface IStreamableType
    {
        void Read(Stream stream);
        void Write(Stream stream);
    }

    // read / write IStreamableType
    partial class StreamExtensions
    {
        public static T[] ReadArray<T>(this Stream stream, ref MyFunc<T> objCreator) where T : IStreamableType
        {
            var length = stream.ReadInt32();
            if (length == -1)
                return null;

            var result = new T[length];
            for (int i = 0; i < length; i++)
            {
                var obj = objCreator();
                obj.Read(stream);
                result[i] = obj;
            }

            return result;
        }

        public static void WriteArray<T>(this Stream stream, T[] objs) where T : IStreamableType
        {
            if (objs == null)
            {
                stream.WriteInt32(-1);
            }
            else
            {
                stream.WriteInt32(objs.Length);
                for (int i = 0; i < objs.Length; i++)
                    objs[i].Write(stream);
            }
        }
    }

    [ComVisible(true)]
    [Serializable]
    public enum KnownTypeCode : byte
    {
        Empty = TypeCode.Empty,
        Object = TypeCode.Object,
        DBNull = TypeCode.DBNull,
        Boolean = TypeCode.Boolean,
        Char = TypeCode.Char,
        SByte = TypeCode.SByte,
        Byte = TypeCode.Byte,
        Int16 = TypeCode.Int16,
        UInt16 = TypeCode.UInt16,
        Int32 = TypeCode.Int32,
        UInt32 = TypeCode.UInt32,
        Int64 = TypeCode.Int64,
        UInt64 = TypeCode.UInt64,
        Single = TypeCode.Single,
        Double = TypeCode.Double,
        Decimal = TypeCode.Decimal,
        DateTime = TypeCode.DateTime,
        String = TypeCode.String,

        Version,
        Guid,
        TimeSpan
    }
}

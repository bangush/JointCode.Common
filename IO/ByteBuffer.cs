//
// Authors:
//   刘静谊 (Johnny Liu) <jingeelio@163.com>
//
// Copyright (c) 2017 刘静谊 (Johnny Liu)
//
// Licensed under the LGPLv3 license. Please see <http://www.gnu.org/licenses/lgpl-3.0.html> for license text.
//

////Based on Stream class of Mono.Cecil, written by Jb Evain (jbevain@gmail.com)

//using System;
//using System.IO;

//namespace JointCode.Common.IO
//{
//    // TODO: 该类实现了一个连续存储块（即只能连续读或写，而不提供 seek 能力）。
//    // 1. 拆分成 ByteBufferReader 和 ByteBufferWriter 两个类（不可拆分，因为可以直接在一个对象实例中完成读写。如果拆分成两个类，则读写需要分配两次内存）
//    // 2. 参考 AltSerializer，一开始就分配一个缓冲大小
//    // 3. 实现对象池以避免频繁分配内存（尤其是在面向服务环境中）

//    /// <summary>
//    /// This class aims to provide a convenient way for sequential reading and writing of small memory blocks (data bytes), 
//    /// with no seek capability and thread safety.
//    /// </summary>
//    public partial class Stream
//    {
//        [ThreadStatic]
//        static BinaryHelper _helper;

//        byte[] _bytes;
//        int _length;
//        int _writePosition;
//        int _readPosition;

//        /// <summary>
//        /// Initializes a new empty buffer with capacity of 4k.
//        /// </summary>
//        public Stream()
//        {
//            _bytes = new byte[4 * 1024];
//        }

//        /// <summary>
//        /// Initializes a new empty buffer with capacity of <paramref name="length"/>.
//        /// </summary>
//        /// <param name="length">The length.</param>
//        public Stream(int length)
//        {
//            _bytes = new byte[length > 0 ? length : 0];
//        }

//        /// <summary>
//        /// Initializes a new instance of the <see cref="Stream"/> class.
//        /// </summary>
//        /// <param name="dataBytes">A byte array that contains data.</param>
//        public Stream(byte[] dataBytes)
//        {
//            _bytes = dataBytes ?? new byte[0];
//            _length = _bytes.Length;
//        }

//        /// <summary>
//        /// Initializes a new instance of the <see cref="Stream"/> class.
//        /// </summary>
//        /// <param name="dataBytes">A byte array that contains data.</param>
//        /// <param name="dataLength">Length of the data.</param>
//        public Stream(byte[] dataBytes, int dataLength)
//        {
//            _bytes = dataBytes ?? new byte[0];
//            _length = dataLength;
//        }

//        #region Extended members

//        /// <summary>
//        /// Gets the write position.
//        /// </summary>
//        public int WritePosition
//        {
//            get { return _writePosition; }
//            //set
//            //{
//            //    if (value > _writePosition)
//            //        throw new ArgumentOutOfRangeException("Can not set the write position after existing position!");
//            //    _writePosition = value;
//            //}
//        }

//        /// <summary>
//        /// Gets the read position.
//        /// </summary>
//        public int ReadPosition
//        {
//            get { return _readPosition; }
//            //set
//            //{
//            //}
//        }

//        /// <summary>
//        /// Gets the data length.
//        /// </summary>
//        public int Length
//        {
//            get { return _length; }
//        }

//        /// <summary>
//        /// Gets the bytes.
//        /// </summary>
//        public byte[] Bytes
//        {
//            get { return _bytes; }
//        }

//        /// <summary>
//        /// Increasing the capacity of inner array with specified amount. 
//        /// This operation will not increment the <see cref="Length"/>, <see cref="ReadPosition"/> 
//        /// or <see cref="WritePosition"/>.
//        /// </summary>
//        /// <param name="amount">The desired amount.</param>
//        public void Expand(int amount)
//        {
//            DoExpand(amount);
//        }

//        public void WriteBytes(Stream buffer)
//        {
//            if (_writePosition + buffer._length > _bytes.Length)
//                DoExpand(buffer._length);

//            Buffer.BlockCopy(buffer._bytes, 0, _bytes, _writePosition, buffer._length);
//            _writePosition += buffer._length;

//            if (_writePosition > _length)
//                _length = _writePosition;
//        }

//        //void WriteBytes(int length)
//        //{
//        //    if (_writePosition + length > _bytes.Length)
//        //        DoExpand(length);

//        //    _writePosition += length;

//        //    if (_writePosition > _length)
//        //        _length = _writePosition;
//        //}

//        byte[] ReadBytes(int length)
//        {
//            var bytes = new byte[length];
//            Buffer.BlockCopy(_bytes, _readPosition, bytes, 0, length);
//            _readPosition += length;
//            return bytes;
//        }

//        void WriteBytes(byte[] srcBuffer)
//        {
//            WriteBytes(srcBuffer, 0, srcBuffer.Length);
//        }

//        void WriteBytes(byte[] srcBuffer, int srcOffset, int length)
//        {
//            if (_writePosition + length > _bytes.Length)
//                DoExpand(length);

//            Buffer.BlockCopy(srcBuffer, srcOffset, _bytes, _writePosition, length);
//            _writePosition += length;

//            if (_writePosition > _length)
//                _length = _writePosition;
//        }

//        #endregion

//        public byte ReadByte()
//        {
//            return _bytes[_readPosition++];
//        }
//        public void WriteByte(byte value)
//        {
//            if (_writePosition == _bytes.Length)
//                DoExpand(1);

//            _bytes[_writePosition++] = value;

//            if (_writePosition > _length)
//                _length = _writePosition;
//        }

//        public sbyte ReadSByte()
//        {
//            return (sbyte)ReadByte();
//        }
//        public void WriteSByte(sbyte value)
//        {
//            WriteByte((byte)value);
//        }

//        public void ReadBytes(byte[] dstBuffer, int length)
//        {
//            Buffer.BlockCopy(_bytes, _readPosition, dstBuffer, 0, length);
//            _readPosition += length;
//        }
//        public void WriteBytes(byte[] srcBuffer, int length)
//        {
//            if (_writePosition + length > _bytes.Length)
//                DoExpand(length);

//            Buffer.BlockCopy(srcBuffer, 0, _bytes, _writePosition, length);
//            _writePosition += length;

//            if (_writePosition > _length)
//                _length = _writePosition;
//        }

//        public ushort ReadUInt16()
//        {
//            var value = (ushort)(_bytes[_readPosition] | (_bytes[_readPosition + 1] << 8));
//            _readPosition += 2;
//            return value;
//        }
//        public void WriteUInt16(ushort value)
//        {
//            if (_writePosition + 2 > _bytes.Length)
//                DoExpand(2);

//            _bytes[_writePosition++] = (byte)value;
//            _bytes[_writePosition++] = (byte)(value >> 8);

//            if (_writePosition > _length)
//                _length = _writePosition;
//        }

//        public short ReadInt16()
//        {
//            return (short)ReadUInt16();
//        }
//        public void WriteInt16(short value)
//        {
//            WriteUInt16((ushort)value);
//        }

//        public uint ReadUInt32()
//        {
//            uint value = (uint)(_bytes[_readPosition]
//                | (_bytes[_readPosition + 1] << 8)
//                | (_bytes[_readPosition + 2] << 16)
//                | (_bytes[_readPosition + 3] << 24));
//            _readPosition += 4;
//            return value;
//        }
//        public void WriteUInt32(uint value)
//        {
//            if (_writePosition + 4 > _bytes.Length)
//                DoExpand(4);

//            _bytes[_writePosition++] = (byte)value;
//            _bytes[_writePosition++] = (byte)(value >> 8);
//            _bytes[_writePosition++] = (byte)(value >> 16);
//            _bytes[_writePosition++] = (byte)(value >> 24);

//            if (_writePosition > _length)
//                _length = _writePosition;
//        }

//#region 此代码结果不正确
//        //public uint ReadCompressedUInt32()
//        //{
//        //    byte first = ReadByte();
//        //    if ((first & 0x80) == 0)
//        //        return first;

//        //    if ((first & 0x40) == 0)
//        //        return ((uint)(first & ~0x80) << 8)
//        //            | ReadByte();

//        //    return ((uint)(first & ~0xc0) << 24)
//        //        | (uint)ReadByte() << 16
//        //        | (uint)ReadByte() << 8
//        //        | ReadByte();
//        //}
//        //public void WriteCompressedUInt32(uint value)
//        //{
//        //    if (value < 0x80)
//        //        WriteByte((byte)value);
//        //    else if (value < 0x4000)
//        //    {
//        //        WriteByte((byte)(0x80 | (value >> 8)));
//        //        WriteByte((byte)(value & 0xff));
//        //    }
//        //    else
//        //    {
//        //        WriteByte((byte)((value >> 24) | 0xc0));
//        //        WriteByte((byte)((value >> 16) & 0xff));
//        //        WriteByte((byte)((value >> 8) & 0xff));
//        //        WriteByte((byte)(value & 0xff));
//        //    }
//        //}  
//        #endregion

//        public int ReadInt32()
//        {
//            return (int)ReadUInt32();
//        }
//        public void WriteInt32(int value)
//        {
//            WriteUInt32((uint)value);
//        }

//#region 此代码结果不正确
//        //public int ReadCompressedInt32()
//        //{
//        //    var value = (int)(ReadCompressedUInt32() >> 1);
//        //    if ((value & 1) == 0)
//        //        return value;
//        //    if (value < 0x40)
//        //        return value - 0x40;
//        //    if (value < 0x2000)
//        //        return value - 0x2000;
//        //    if (value < 0x10000000)
//        //        return value - 0x10000000;
//        //    return value - 0x20000000;
//        //}
//        //public void WriteCompressedInt32(int value)
//        //{
//        //    if (value >= 0)
//        //    {
//        //        WriteCompressedUInt32((uint)(value << 1));
//        //        return;
//        //    }

//        //    if (value > -0x40)
//        //        value = 0x40 + value;
//        //    else if (value >= -0x2000)
//        //        value = 0x2000 + value;
//        //    else if (value >= -0x20000000)
//        //        value = 0x20000000 + value;

//        //    WriteCompressedUInt32((uint)((value << 1) | 1));
//        //}  
//#endregion

//        public ulong ReadUInt64()
//        {
//            uint low = ReadUInt32();
//            uint high = ReadUInt32();

//            return (((ulong)high) << 32) | low;
//        }
//        public void WriteUInt64(ulong value)
//        {
//            if (_writePosition + 8 > _bytes.Length)
//                DoExpand(8);

//            _bytes[_writePosition++] = (byte)value;
//            _bytes[_writePosition++] = (byte)(value >> 8);
//            _bytes[_writePosition++] = (byte)(value >> 16);
//            _bytes[_writePosition++] = (byte)(value >> 24);
//            _bytes[_writePosition++] = (byte)(value >> 32);
//            _bytes[_writePosition++] = (byte)(value >> 40);
//            _bytes[_writePosition++] = (byte)(value >> 48);
//            _bytes[_writePosition++] = (byte)(value >> 56);

//            if (_writePosition > _length)
//                _length = _writePosition;
//        }

//        public long ReadInt64()
//        {
//            return (long)ReadUInt64();
//        }
//        public void WriteInt64(long value)
//        {
//            WriteUInt64((ulong)value);
//        }

//        public float ReadSingle()
//        {
//            if (!BitConverter.IsLittleEndian)
//            {
//                var bytes = ReadBytes(4);
//                Array.Reverse(bytes);
//                return BitConverter.ToSingle(bytes, 0);
//            }

//            float value = BitConverter.ToSingle(_bytes, _readPosition);
//            _readPosition += 4;
//            return value;
//        }
//        public void WriteSingle(float value)
//        {
//            var bytes = BitConverter.GetBytes(value);

//            if (!BitConverter.IsLittleEndian)
//                Array.Reverse(bytes);

//            WriteBytes(bytes);
//        }

//        public double ReadDouble()
//        {
//            if (!BitConverter.IsLittleEndian)
//            {
//                var bytes = ReadBytes(8);
//                Array.Reverse(bytes);
//                return BitConverter.ToDouble(bytes, 0);
//            }

//            double value = BitConverter.ToDouble(_bytes, _readPosition);
//            _readPosition += 8;
//            return value;
//        }
//        public void WriteDouble(double value)
//        {
//            var bytes = BitConverter.GetBytes(value);

//            if (!BitConverter.IsLittleEndian)
//                Array.Reverse(bytes);

//            WriteBytes(bytes);
//        }

//        /// <summary>
//        /// Increasing the capacity of inner array with specified amount. 
//        /// This operation will not increment the data length.
//        /// </summary>
//        /// <param name="amount">The desired amount.</param>
//        void DoExpand(int amount)
//        {
//            var currentBytes = _bytes;
//            var currentLength = currentBytes.Length;

//            var newBytes = new byte[Math.Max(currentLength + amount, currentLength * 2)];
//            Buffer.BlockCopy(currentBytes, 0, newBytes, 0, currentLength);
//            _bytes = newBytes;
//        }
//    }

//    partial class Stream
//    {
//        /// <summary>
//        /// Reads a string from the buffer.
//        /// </summary>
//        public string ReadString()
//        {
//            int totalBytes = ReadUInt24();

//            if (totalBytes == 0xFFFFFF)
//                // null was written.
//                return null;

//            var helper = _helper;
//            if (helper == null)
//                _helper = helper = new BinaryHelper();

//            if (totalBytes > BinaryHelper.ByteBufferSize)
//            {
//                var bytes = new byte[totalBytes];
//                ReadBytes(bytes, totalBytes);
//                return helper.Encoding.GetString(bytes, 0, totalBytes);
//            }

//            ReadBytes(helper.Stream, totalBytes);
//            return helper.Encoding.GetString(helper.Stream, 0, totalBytes);
//        }
//        // Read 24-bit unsigned int
//        int ReadUInt24()
//        {
//            // Read first two bytes.
//            int newValue = ReadByte();
//            newValue += (ReadByte() << 8);
//            newValue += (ReadByte() << 16);
//            return newValue;
//        }
//        /// <summary>
//        /// Writes a string to the buffer.
//        /// </summary>
//        public void WriteString(string value)
//        {
//            if (value == null)
//            {
//                // write max 24-bit to indicate null.
//                WriteUInt24(0xFFFFFF);
//                return;
//            }

//            var helper = _helper;
//            if (helper == null)
//                _helper = helper = new BinaryHelper();

//            // Write the size of the string
//            var totalBytes = helper.Encoding.GetByteCount(value);
//            WriteUInt24(totalBytes);

//            if (totalBytes > BinaryHelper.ByteBufferSize)
//            {
//                var bytes = new byte[totalBytes];
//                helper.Encoding.GetBytes(value, 0, value.Length, bytes, 0);
//                WriteBytes(bytes, totalBytes);
//            }
//            else
//            {
//                helper.Encoding.GetBytes(value, 0, value.Length, helper.Stream, 0);
//                WriteBytes(helper.Stream, totalBytes);
//            }
//        }
//        // Write 24-bit unsigned int
//        void WriteUInt24(int value)
//        {
//            WriteByte((byte)(value & 0xFF));
//            WriteByte((byte)(((value >> 8) & 0xFF)));
//            WriteByte((byte)(((value >> 16) & 0xFF)));
//        }

//        /// <summary>
//        /// Reads a Char from the buffer.
//        /// </summary>
//        public char ReadChar()
//        {
//            var bytes = ReadBytes(2);
//            return BitConverter.ToChar(bytes, 0);
//        }
//        /// <summary>
//        /// Writes a Char to the buffer.
//        /// </summary>
//        public void WriteChar(char value)
//        {
//            WriteBytes(BitConverter.GetBytes(value), 0, 2);
//        }

//        /// <summary>
//        /// Reads a Decimal from the buffer.
//        /// </summary>
//        public decimal ReadDecimal()
//        {
//            var newDecimal = new Int32[4];
//            newDecimal[0] = ReadInt32();
//            newDecimal[1] = ReadInt32();
//            newDecimal[2] = ReadInt32();
//            newDecimal[3] = ReadInt32();
//            return new decimal(newDecimal);
//        }
//        /// <summary>
//        /// Writes a Decimal to the buffer.
//        /// </summary>
//        public void WriteDecimal(decimal value)
//        {
//            int[] bits = decimal.GetBits(value);
//            WriteInt32(bits[0]);
//            WriteInt32(bits[1]);
//            WriteInt32(bits[2]);
//            WriteInt32(bits[3]);
//        }

//        /// <summary>
//        /// Reads a Byte from the buffer.
//        /// </summary>
//        public bool ReadBoolean()
//        {
//            var b = ReadByte();
//            if (b == 0) return false;
//            if (b == 1) return true;
//            throw new IOException("Data broken!");
//        }
//        /// <summary>
//        /// Writes a byte to the buffer.
//        /// </summary>
//        public void WriteBoolean(bool value)
//        {
//            if (value) WriteByte(1);
//            else WriteByte(0);
//        }

//        /// <summary>
//        /// Reads a DateTime from the buffer.
//        /// </summary>
//        public DateTime ReadDateTime()
//        {
//            long value = ReadInt64();
//#if MONO
//            return new DateTime(value);
//#else
//            return DateTime.FromBinary(value);
//#endif
//        }
//        /// <summary>
//        /// Writes a DateTime to the buffer.
//        /// </summary>
//        public void WriteDateTime(DateTime value)
//        {
//#if MONO
//            long binary = value.ToFileTime();
//#else
//            long binary = value.ToBinary();
//#endif
//            WriteInt64(binary);
//        }

//        /// <summary>
//        /// Reads a TimeSpan from the buffer.
//        /// </summary>
//        public TimeSpan ReadTimeSpan()
//        {
//            Int64 value = ReadInt64();
//            return new TimeSpan(value);
//        }
//        /// <summary>
//        /// Writes a TimeSpan to the buffer.
//        /// </summary>
//        public void WriteTimeSpan(TimeSpan value)
//        {
//            WriteInt64(value.Ticks);
//        }

//        /// <summary>
//        /// Reads a Guid from the buffer.
//        /// </summary>
//        public Guid ReadGuid()
//        {
//            var bytes = ReadBytes(16);
//            return new Guid(bytes);
//        }
//        /// <summary>
//        /// Writes a Guid to the buffer.
//        /// </summary>
//        public void WriteGuid(Guid value)
//        {
//            WriteBytes(value.ToByteArray());
//        }

//        public Version ReadVersion()
//        {
//            int major = ReadInt32();
//            int minor = ReadInt32();
//            int build = ReadInt32();
//            int revision = ReadInt32();
//            if (major == -1 || minor == -1)
//                return null;

//            Version ver;
//            if (build == -1)
//                ver = new Version(major, minor);
//            else
//                ver = revision == -1
//                    ? new Version(major, minor, build)
//                    : new Version(major, minor, build, revision);
//            return ver;
//        }
//        public void WriteVersion(Version value)
//        {
//            if (value == null || value.Major < 0 || value.Minor < 0)
//            {
//                WriteInt32(-1);
//                WriteInt32(-1);
//                WriteInt32(-1);
//                WriteInt32(-1);
//            }
//            else
//            {
//                WriteInt32(value.Major);
//                WriteInt32(value.Minor);
//                if (value.Build < 0)
//                {
//                    WriteInt32(-1);
//                    WriteInt32(-1);
//                }
//                else
//                {
//                    WriteInt32(value.Build);
//                    if (value.Revision < 0)
//                        WriteInt32(-1);
//                    else
//                        WriteInt32(value.Revision);
//                }
//            }
//        }

//        public TEnum ReadEnum<TEnum>() where TEnum : struct 
//        {
//            throw new NotImplementedException();
//        }
//        public void WriteEnum<TEnum>(TEnum value) where TEnum : struct 
//        {
//            throw new NotImplementedException();
//        }
//    }
//}

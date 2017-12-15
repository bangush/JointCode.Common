//#define NO_UNSAFE

//
// Authors:
//   刘静谊 (Johnny Liu) <jingeelio@163.com>
//
// Copyright (c) 2017 刘静谊 (Johnny Liu)
//
// Licensed under the LGPLv3 license. Please see <http://www.gnu.org/licenses/lgpl-3.0.html> for license text.
//


using System.Security;
using System.Text;

#if ENCRYPT
namespace JointCode.Internals
#else
namespace JointCode.Common.IO
#endif

{
    partial class BinaryHelper
    {
        internal const int ByteBufferSize = 256; // pre-allocate 256 bytes memory
        static readonly byte[] _emptyByteBuffer = new byte[0];
        readonly Encoding _utf8Encoding = new UTF8Encoding(false, true);
        byte[] _byteBuffer;

        internal static byte[] EmptyByteBuffer { get { return _emptyByteBuffer; } }
        internal Encoding Encoding { get { return _utf8Encoding; } }
        internal byte[] Stream { get { if (_byteBuffer == null) _byteBuffer = new byte[ByteBufferSize]; return _byteBuffer; } }
    }

#if !NO_UNSAFE
    partial class BinaryHelper
    {
        internal const int CharBufferSize = 128;
        Encoder _encoder;
        Decoder _decoder;
        char[] _charBuffer;

        internal Encoder Encoder { get { if (_encoder == null) _encoder = Encoding.GetEncoder(); return _encoder; } }
        internal Decoder Decoder { get { if (_decoder == null) _decoder = Encoding.GetDecoder(); return _decoder; } }
        internal char[] CharBuffer { get { if (_charBuffer == null) _charBuffer = new char[CharBufferSize]; return _charBuffer; } }

        [SecuritySafeCritical]
        internal static unsafe int ToInt(float f)
        {
            return *(int*)(&f);
        }

        [SecuritySafeCritical]
        internal static unsafe long ToLong(double f)
        {
            return *(long*)(&f);
        }

        [SecuritySafeCritical]
        internal static unsafe float ToSingle(int i)
        {
            return *(float*)(&i);
        }

        [SecuritySafeCritical]
        internal static unsafe double ToDouble(long i)
        {
            return *(double*)(&i);
        }
    }
#endif
}
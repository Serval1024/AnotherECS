using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using AnotherECS.Exceptions;
using AnotherECS.Serializer;
using AnotherECS.Unsafe;

namespace AnotherECS.Core.Collection
{
    public unsafe struct NStream<TAllocator> : INative, ISerialize, IEnumerable<byte>, IRebindMemoryHandle
        where TAllocator : unmanaged, IAllocator
    {
        private NArray<TAllocator, byte> _data;
        private uint _position;

        public bool IsValide
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _data.IsValide;
        }

        public uint Position
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _position;
        }

        public NStream(TAllocator* allocator, ref MemoryHandle memoryHandle, uint elementCount)
        {
            _data = new NArray<TAllocator, byte>(allocator, ref memoryHandle, elementCount);
            _position = 0;
        }

        public NStream(TAllocator* allocator, uint elementCount)
        {
            _data = new NArray<TAllocator, byte>(allocator, elementCount);
            _position = 0;
        }

        public NStream(NArray<TAllocator, byte> source)
        {
            _data = default;
            _data.CreateFrom(source);
            _position = 0;
        }

        public NStream(TAllocator* allocator, byte[] source)
        {
            this = new NStream<TAllocator>(allocator, (uint)source.Length);
            Marshal.Copy(source, 0, (IntPtr)_data.ReadPtr(), source.Length);
        }

        #region Write
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(byte value)
        {
            TryResize(sizeof(byte));
            WriteIternal(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(bool value)
        {
            TryResize(sizeof(bool));
            WriteIternal((byte)(value ? 1u : 0u));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(sbyte value)
        {
            TryResize(sizeof(sbyte));
            WriteIternal((byte)value);
        }

        public void Write(float value)
        {
            TryResize(sizeof(float));
            uint num = *(uint*)(&value);
            WriteIternal((byte)num);
            WriteIternal((byte)(num >> 8));
            WriteIternal((byte)(num >> 16));
            WriteIternal((byte)(num >> 24));
        }

        public void Write(double value)
        {
            TryResize(sizeof(double));
            ulong num = *(ulong*)(&value);
            WriteIternal((byte)num);
            WriteIternal((byte)(num >> 8));
            WriteIternal((byte)(num >> 16));
            WriteIternal((byte)(num >> 24));
            WriteIternal((byte)(num >> 32));
            WriteIternal((byte)(num >> 40));
            WriteIternal((byte)(num >> 48));
            WriteIternal((byte)(num >> 56));
        }

        public void Write(short value)
        {
            TryResize(sizeof(short));
            WriteIternal((byte)value);
            WriteIternal((byte)(value >> 8));
        }

        public void Write(ushort value)
        {
            TryResize(sizeof(ushort));
            WriteIternal((byte)value);
            WriteIternal((byte)(value >> 8));
        }

        public void Write(int value)
        {
            TryResize(sizeof(int));
            WriteIternal((byte)value);
            WriteIternal((byte)(value >> 8));
            WriteIternal((byte)(value >> 16));
            WriteIternal((byte)(value >> 24));
        }

        public void Write(uint value)
        {
            TryResize(sizeof(uint));
            WriteIternal((byte)value);
            WriteIternal((byte)(value >> 8));
            WriteIternal((byte)(value >> 16));
            WriteIternal((byte)(value >> 24));
        }

        public void Write(long value)
        {
            TryResize(sizeof(long));
            WriteIternal((byte)value);
            WriteIternal((byte)(value >> 8));
            WriteIternal((byte)(value >> 16));
            WriteIternal((byte)(value >> 24));
            WriteIternal((byte)(value >> 32));
            WriteIternal((byte)(value >> 40));
            WriteIternal((byte)(value >> 48));
            WriteIternal((byte)(value >> 56));
        }

        public void Write(ulong value)
        {
            TryResize(sizeof(ulong));
            WriteIternal((byte)value);
            WriteIternal((byte)(value >> 8));
            WriteIternal((byte)(value >> 16));
            WriteIternal((byte)(value >> 24));
            WriteIternal((byte)(value >> 32));
            WriteIternal((byte)(value >> 40));
            WriteIternal((byte)(value >> 48));
            WriteIternal((byte)(value >> 56));
        }

        public void Write(byte[] buffer)
        {
            TryResize(buffer.Length * sizeof(sbyte));
            Write(buffer, 0, (uint)buffer.Length);
        }

        public void Write(string @value)
        {
            var bytes = Encoding.UTF8.GetBytes(@value);
            Write((ushort)bytes.Length);
            Write(bytes);
        }

        public void Write(byte[] buffer, uint start, uint count)
        {
            if (count + start >= buffer.Length)
            {
                throw new IndexOutOfRangeException($"{nameof(start)} or {nameof(count)}");
            }

            TryResize((int)count * sizeof(sbyte));
            for (uint i = start, iMax = count + start; i < iMax; ++i)
            {
                WriteIternal(buffer[i]);
            }
        }

        public void Write(byte* ptr, uint length)
        {
            TryResize((int)length * sizeof(byte));

            Unsafe.UnsafeMemory.MemCopy(_data.GetPtr() + _position, ptr, length);
            _position += length;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void WriteIternal(byte value)
        {
            _data.GetRef(_position++) = value;
        }
        #endregion

        #region Read
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte ReadByte()
        {
            CheakRead(sizeof(byte));
            return ReadByteIternal();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public sbyte ReadSByte()
        {
            CheakRead(sizeof(sbyte));
            return (sbyte)ReadByteIternal();
        }

        public short ReadInt16()
        {
            CheakRead(sizeof(short));
            return (short)(ReadByteIternal() | (ReadByteIternal() << 8));
        }

        public ushort ReadUInt16()
        {
            CheakRead(sizeof(ushort));
            return (ushort)(ReadByteIternal() | (ReadByteIternal() << 8));
        }

        public int ReadInt32()
        {
            CheakRead(sizeof(int));
            return (int)(ReadByteIternal() | (ReadByteIternal() << 8) | (ReadByteIternal() << 16) | (ReadByteIternal() <<24));
        }

        public uint ReadUInt32()
        {
            CheakRead(sizeof(uint));
            return (uint)(ReadByteIternal() | (ReadByteIternal() << 8) | (ReadByteIternal() << 16) | (ReadByteIternal() << 24));
        }

        public long ReadInt64()
        {
            CheakRead(sizeof(long));
            return (long)(ReadByteIternal() | (ReadByteIternal() << 8) | (ReadByteIternal() << 16) | (ReadByteIternal() << 24) | (ReadByteIternal() << 32) | (ReadByteIternal() << 40) | (ReadByteIternal() << 48) | (ReadByteIternal() << 56));
        }

        public ulong ReadUInt64()
        {
            CheakRead(sizeof(ulong));
            return (ulong)(ReadByteIternal() | (ReadByteIternal() << 8) | (ReadByteIternal() << 16) | (ReadByteIternal() << 24) | (ReadByteIternal() << 32) | (ReadByteIternal() << 40) | (ReadByteIternal() << 48) | (ReadByteIternal() << 56));
        }

        public float ReadSingle()
        {
            uint num = (uint)(ReadByteIternal() | (ReadByteIternal() << 8) | (ReadByteIternal() << 16) | (ReadByteIternal() << 24));
            return *(float*)(&num);
        }

        public double ReadDouble()
        {
            ulong num = (ulong)(ReadByteIternal() | (ReadByteIternal() << 8) | (ReadByteIternal() << 16) | (ReadByteIternal() << 24) | (ReadByteIternal() << 32) | (ReadByteIternal() << 40) | (ReadByteIternal() << 48) | (ReadByteIternal() << 56));
            return *(double*)(&num);
        }

        public byte[] ReadBytes(uint count)
        {
            var buffer = new byte[count];
            Read(buffer);
            return buffer;
        }

        public void Read(byte[] buffer)
        {
            Read(buffer, 0, (uint)buffer.Length);
        }

        public void Read(byte[] buffer, uint start, uint count)
        {
            if (count + start >= buffer.Length)
            {
                throw new IndexOutOfRangeException($"{nameof(start)} or {nameof(count)}");
            }

            CheakRead((int)count * sizeof(byte));
            for (uint i = start, iMax = count + start; i < iMax; ++i)
            {
                buffer[i] = ReadByteIternal();
            }
        }

        public string ReadString()
        {
            var count = ReadUInt16();
            var bytes = ReadBytes(count);
            return Encoding.UTF8.GetString(bytes);
        }

        public void Read(byte* ptr, uint length)
        {
            CheakRead((int)length * sizeof(byte));

            UnsafeMemory.MemCopy(ptr, _data.ReadPtr() + _position, length);
            _position += length;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private byte ReadByteIternal()
            => _data.ReadRef(_position++);
        #endregion

        public byte[] ToArray()
        {
            var result = new byte[_position];
            Marshal.Copy((IntPtr)_data.ReadPtr(), result, 0, result.Length);
            return result;
        }

        public NArray<TAllocator, byte> ToNArray()
        {
            var result = new NArray<TAllocator, byte>(_data.GetAllocator(), _position);
            UnsafeMemory.MemCopy(result.ReadPtr(), _data.ReadPtr(), _position);
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            _data.Dispose();
            _position = 0;
        }

        public IEnumerator<byte> GetEnumerator()
            => _data.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => _data.GetEnumerator();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void IRebindMemoryHandle.RebindMemoryHandle(ref MemoryRebinderContext rebinder)
        {
            MemoryRebinderCaller.Rebind(ref _data, ref rebinder);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void CheakRead(int typeSize)
        {
            if (_position + typeSize > _data.Length)
            {
                throw new EndOfStreamException();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void TryResize(int typeSize)
        {
            var size = _position + (uint)typeSize;
            if (size > _data.Length)
            {
                _data.Resize(size > (_data.Length << 1) ? size : (_data.Length << 1));
            }
        }

        public void Pack(ref WriterContextSerializer writer)
        {
            writer.Write(_position);
            _data.Pack(ref writer);
        }

        public void Unpack(ref ReaderContextSerializer reader)
        {
            _position = reader.ReadUInt32();
            _data.Unpack(ref reader);
        }
    }
}

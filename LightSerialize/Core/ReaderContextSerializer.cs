using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using AnotherECS.Serializer.Exceptions;

namespace AnotherECS.Serializer
{
    public struct ReaderContextSerializer : IDisposable
    {
        private readonly LightSerializer _serializer;
        private readonly MemoryStream _stream;
        private readonly BinaryReader _reader;

        public ReaderContextSerializer(LightSerializer serializer, byte[] data)
        {
            _serializer = serializer;
            _stream = new MemoryStream(data);
            _reader = new BinaryReader(_stream);
        }

        public void Dispose()
        {
            _reader.BaseStream.Dispose();
            _reader.Dispose();
        }

        public long Position
            => _reader.BaseStream.Position;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Type IdToType(uint id)
        {
            try
            {
                return _serializer.IdToType(id);
            }
            catch (KeyNotFoundException e)
            {
                throw new UnknowTypeException(id, e);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool GetSerializer(Type type, out IElementSerializer serializer)
            => _serializer.GetSerializer(type, out serializer);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint TypeToId(Type type)
        {
            try
            {
                return _serializer.TypeToId(type);
            }
            catch (KeyNotFoundException e)
            {
                throw new UnknowTypeException(type, e);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public object Unpack(params object[] constructArgs)
            => _serializer.Unpack(ref this, constructArgs);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Unpack<T>(params object[] constructArgs)
        {
            var unpack = Unpack(constructArgs);
            return (unpack == null) 
                ? default 
                : (T)unpack;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public object ReadStruct(Type type)
            => _serializer.ReadStruct(ref this, type);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T ReadStruct<T>()
            => _serializer.ReadStruct<T>(ref this);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Read()
            => _reader.Read();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ReadBoolean()
            => _reader.ReadBoolean();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte ReadByte()
            => _reader.ReadByte();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public sbyte ReadSByte()
            => _reader.ReadSByte();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public char ReadChar()
            => _reader.ReadChar();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public short ReadInt16()
            => _reader.ReadInt16();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ushort ReadUInt16()
            => _reader.ReadUInt16();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int ReadInt32()
            => _reader.ReadInt32();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint ReadUInt32()
            => _reader.ReadUInt32();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public long ReadInt64()
            => _reader.ReadInt64();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ulong ReadUInt64()
            => _reader.ReadUInt64();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float ReadSingle()
            => _reader.ReadSingle();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double ReadDouble()
            => _reader.ReadDouble();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public decimal ReadDecimal()
            => _reader.ReadDecimal();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string ReadString()
            => _reader.ReadString();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Read(char[] buffer, int index, int count)
            => _reader.Read(buffer, index, count);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public char[] ReadChars(int count)
            => _reader.ReadChars(count);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Read(byte[] buffer, int index, int count)
            => _reader.Read(buffer, index, count);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte[] ReadBytes(int count)
            => ReadBytes(count);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string ReadUTF8()
           => _reader.ReadUTF8();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void Read(void* buffer, uint length)
           => _reader.ReadBytePtr((byte*)buffer, length);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T[] ReadUnmanagedArray<T>()
           where T : unmanaged
           => _serializer.ReadUnmanaged<T>(ref this);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T[] ReadArray<T>()
           where T : struct
           => _serializer.ReadArray<T>(ref this);
    }
}


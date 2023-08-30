using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using AnotherECS.Serializer.Exceptions;

namespace AnotherECS.Serializer
{
    public struct WriterContextSerializer : IDisposable
    {
        private readonly LightSerializer _serializer;
        private readonly MemoryStream _stream;
        private readonly BinaryWriter _writer;
        
        public WriterContextSerializer(LightSerializer serializer)
        {
            _serializer = serializer;
            _stream = new MemoryStream();
            _writer = new BinaryWriter(_stream);
        }

        public void Dispose()
        {
            _writer.BaseStream.Dispose();
            _writer.Dispose();
        }

        public long Position
            => _writer.BaseStream.Position;

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
        public bool GetSerializer(Type type, out IElementSerializer serializer)
            => _serializer.GetSerializer(type, out serializer);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte[] ToArray()
            => _stream.ToArray();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Pack(object value)
            => _serializer.Pack(ref this, value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteStruct(Type type, object value)
            => _serializer.WriteStruct(ref this, type, value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteStruct(object value)
            => _serializer.WriteStruct(ref this, value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(bool value)
            => _writer.Write(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(byte value)
            => _writer.Write(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(sbyte value)
            => _writer.Write(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(byte[] value)
            => _writer.Write(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(byte[] buffer, int index, int count)
            => _writer.Write(buffer, index, count);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(char value)
            => _writer.Write(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(char[] value)
            => _writer.Write(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(char[] value, int index, int count)
            => _writer.Write(value, index, count);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(double value)
            => _writer.Write(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(decimal value)
            => _writer.Write(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(short value)
            => _writer.Write(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(ushort value)
            => _writer.Write(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(int value)
            => _writer.Write(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(uint value)
            => _writer.Write(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(long value)
            => _writer.Write(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(ulong value)
            => _writer.Write(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(float value)
            => _writer.Write(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(string value)
            => _writer.Write(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUTF8(string value)
            => _writer.WriteUTF8(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void Write(void* value, uint length)
            => _writer.Write((byte*)value, length);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUnmanagedArray<T>(T[] value)
            where T : unmanaged
            => _serializer.WriteUnmanaged(ref this, value, (value == null) ? 0 : value.Length);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUnmanagedArray<T>(T[] value, int count)
            where T : unmanaged
            => _serializer.WriteUnmanaged(ref this, value, count);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteArray<T>(T[] value, int count)
            where T : struct
            => _serializer.WriteArray(ref this, value, count);
    }
}

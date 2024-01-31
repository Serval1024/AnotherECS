using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using AnotherECS.Serializer.Exceptions;

namespace AnotherECS.Serializer
{
    public struct WriterContextSerializer : IDisposable
    {
        private const uint INIT_CAPACITY = 1024;

        private readonly LightSerializer _serializer;
        private Stream _stream;
        private Dictionary<Type, Dictionary<uint, object>> _dependencies;

        public WriterContextSerializer(LightSerializer serializer, IEnumerable<(uint, object)> dependencies)
        {
            _serializer = serializer;
            _stream = new Stream(INIT_CAPACITY);

            _dependencies = new Dictionary<Type, Dictionary<uint, object>>();
            if (dependencies != null)
            {
                foreach (var dependency in dependencies)
                {
                    AddDependency(dependency.Item1, dependency.Item2);
                }
            }
        }

        public void Dispose()
        {
            _stream.Dispose();
        }

        public uint Position
            => _stream.Position;

        public void AddDependency<T>(T dependency)
        {
            AddDependencyInternal(typeof(T), 0, dependency);
        }

        public void AddDependency<T>(uint dependencyId, T dependency)
        {
            AddDependencyInternal(typeof(T), dependencyId, dependency);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T GetDependency<T>(uint dependencyId)
          => (T)_dependencies[typeof(T)][dependencyId];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T GetDependency<T>()
            => (T)_dependencies[typeof(T)][0];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void AddDependencyInternal(Type type, uint dependencyId, object dependency)
        {
            if (_dependencies.TryGetValue(type, out var dict))
            {
                dict.Add(dependencyId, dependency);
            }
            else
            {
                _dependencies.Add(type, new Dictionary<uint, object>() { { dependencyId, dependency } });
            }
        }

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
        public void WriteStruct<T>(T value)
            where T : struct
            => _serializer.WriteStruct(ref this, value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void Write<T>(T value)
            where T : unmanaged, Enum
        {
            if (sizeof(T) == 1)
            {
                Write(*(byte*)&value);
            }
            else if (sizeof(T) == 2)
            {
                Write(*(ushort*)&value);
            }
            else if (sizeof(T) == 4)
            {
                Write(*(uint*)&value);
            }
            else if (sizeof(T) == 8)
            {
                Write(*(ulong*)&value);
            }
            else
            {
                throw new InvalidOperationException();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(bool value)
            => _stream.Write(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(byte value)
            => _stream.Write(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(sbyte value)
            => _stream.Write(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(byte[] value)
            => _stream.Write(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(byte[] buffer, uint index, uint count)
            => _stream.Write(buffer, index, count);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(char value)
            => _stream.Write(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(double value)
            => _stream.Write(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(short value)
            => _stream.Write(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(ushort value)
            => _stream.Write(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(int value)
            => _stream.Write(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(uint value)
        {
            default(UInt32Serializer).PackConcrete(ref this, value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(long value)
            => _stream.Write(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(ulong value)
            => _stream.Write(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(float value)
            => _stream.Write(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(string value)
            => _stream.Write(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void Write(void* value, uint length)
            => _stream.Write((byte*)value, length);

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

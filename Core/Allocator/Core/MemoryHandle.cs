using System.Runtime.CompilerServices;
using AnotherECS.Serializer;

namespace AnotherECS.Core
{
    public unsafe struct MemoryHandle : ISerialize
    {
        internal void* pointer;
        internal int* isNotDirty;
        internal ushort chunk;
        internal ushort segment;

        public bool IsValid
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => GetPtr() != null;
        }
        public bool IsDirty
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => *isNotDirty == 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void* GetPtr()
            => pointer;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Pack(ref WriterContextSerializer writer)
        {
            writer.Write(chunk);
            writer.Write(segment);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Unpack(ref ReaderContextSerializer reader)
        {
            chunk = reader.ReadUInt16();
            segment = reader.ReadUInt16();
        }
    }
}
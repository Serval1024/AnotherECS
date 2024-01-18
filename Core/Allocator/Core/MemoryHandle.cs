using System.Runtime.CompilerServices;
using AnotherECS.Serializer;

namespace AnotherECS.Core
{
    public unsafe struct MemoryHandle : ISerialize
    {
        //internal uint __MEMORY_MARKER = 0xAAAA_AAAA; //TODO maybe by marker rebind
        internal void* pointer;
        internal bool* isNotDirty;
        internal uint id;

        public bool IsValid
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => pointer != null;
        }
        public bool IsDirty
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => !*isNotDirty;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void* GetPtr()
        {
#if !ANOTHERECS_RELEASE
            if (pointer == null)
            {
                throw new System.NullReferenceException();
            }
#endif
            return pointer;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Pack(ref WriterContextSerializer writer)
        {
            writer.Write(id);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Unpack(ref ReaderContextSerializer reader)
        {
            id = reader.ReadUInt32();
        }
    }
}
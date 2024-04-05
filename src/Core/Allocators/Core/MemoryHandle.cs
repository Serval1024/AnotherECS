using AnotherECS.Serializer;
using System.Runtime.CompilerServices;

namespace AnotherECS.Core.Allocators
{
    //[StructLayout(LayoutKind.Explicit)]
    public unsafe struct MemoryHandle : ISerialize
    {
        internal void* pointer;
        internal bool* isNotDirty;
        internal uint id;
        /*
        [FieldOffset(0)] internal void* pointer;
        [FieldOffset(8)] internal bool* isNotDirty;
        [FieldOffset(16)] internal uint __MEMORY_MARKER0;// = 0xAAAA_AAAA; //TODO maybe repair by mem marker
        [FieldOffset(20)] internal uint id;
        */
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
using System.Collections.Generic;

namespace AnotherECS.Core.Collection
{
    public unsafe interface INArray : INative
    {
        uint ByteLength { get; }
        uint Length { get; }
        uint ElementSize { get; }
        bool IsDirty { get; }
        void Dirty();
    }

    public unsafe interface INArray<T> : INArray, IEnumerable<T>
       where T : unmanaged
    {
        void Resize(uint elementCount);
        T* GetPtr();
        T* ReadPtr();
        T Read(uint index);
        ref T ReadRef(uint index);
        T Get(uint index);
        ref T GetRef(uint index);
        void Set(uint index, T value);
        void Set(uint index, ref T value);

        T Read(int index);
        ref T ReadRef(int index);
        T Get(int index);
        ref T GetRef(int index);
        void Set(int index, T value);
        void Set(int index, ref T value);
    }
}

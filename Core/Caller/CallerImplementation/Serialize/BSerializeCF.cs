using AnotherECS.Core.Actions;
using AnotherECS.Core.Allocators;
using AnotherECS.Serializer;
using System.Runtime.CompilerServices;

namespace AnotherECS.Core.Caller
{
    internal unsafe struct BSerializeCF<TAllocator, TSparse, TDense, TDenseIndex> : ICallerSerialize<TAllocator, TSparse, TDense, TDenseIndex>, IBoolConst
        where TAllocator : unmanaged, IAllocator
        where TSparse : unmanaged
        where TDense : unmanaged
        where TDenseIndex : unmanaged
    {
        public bool Is { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => false; }

        public void Pack(ref WriterContextSerializer writer, ULayout<TAllocator, TSparse, TDense, TDenseIndex>* layout)
        {
            SerializeActions.PackStorageBlittable(ref writer, layout);
        }

        public void Unpack(ref ReaderContextSerializer reader, ULayout<TAllocator, TSparse, TDense, TDenseIndex>* layout)
        {
            SerializeActions.UnpackStorageBlittable(ref reader, layout);
        }
    }
}

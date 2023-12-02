using System.Runtime.CompilerServices;
using AnotherECS.Core.Actions;
using AnotherECS.Serializer;

namespace AnotherECS.Core.Caller
{
    internal unsafe struct SBSerialize<TSparse, TDense, TDenseIndex, TTickData> : ICustomSerialize<TSparse, TDense, TDenseIndex, TTickData>, IBoolConst
       where TSparse : unmanaged
       where TDense : unmanaged, ISerialize
       where TDenseIndex : unmanaged
       where TTickData : unmanaged
    {
        public bool Is { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => true; }

        public void Pack(ref WriterContextSerializer writer, UnmanagedLayout<TSparse, TDense, TDenseIndex, TTickData>* layout)
        {
            SerializeActions.PackStorageSerialize(ref writer, layout);
            SerializeActions.PackHistoryBlittable(ref writer, layout);
        }

        public void Unpack(ref ReaderContextSerializer reader, UnmanagedLayout<TSparse, TDense, TDenseIndex, TTickData>* layout)
        {
            SerializeActions.UnpackStorageSerialize(ref reader, layout);
            SerializeActions.UnpackHistoryBlittable(ref reader, layout);
        }
    }
}

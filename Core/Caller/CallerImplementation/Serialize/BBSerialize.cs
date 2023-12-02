using System.Runtime.CompilerServices;
using AnotherECS.Core.Actions;
using AnotherECS.Serializer;

namespace AnotherECS.Core.Caller
{
    internal unsafe struct BBSerialize<TSparse, TDense, TDenseIndex, TTickData> : ICustomSerialize<TSparse, TDense, TDenseIndex, TTickData>, IBoolConst
        where TSparse : unmanaged
        where TDense : unmanaged
        where TDenseIndex : unmanaged
        where TTickData : unmanaged
    {
        public bool Is { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => false; }

        public void Pack(ref WriterContextSerializer writer, UnmanagedLayout<TSparse, TDense, TDenseIndex, TTickData>* layout)
        {
            SerializeActions.PackStorageBlittable(ref writer, layout);
            SerializeActions.PackHistoryBlittable(ref writer, layout);
        }

        public void Unpack(ref ReaderContextSerializer reader, UnmanagedLayout<TSparse, TDense, TDenseIndex, TTickData>* layout)
        {
            SerializeActions.UnpackStorageBlittable(ref reader, layout);
            SerializeActions.UnpackHistoryBlittable(ref reader, layout);
        }
    }
}

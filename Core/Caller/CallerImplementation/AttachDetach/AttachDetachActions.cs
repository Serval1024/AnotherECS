using System;
using System.Runtime.CompilerServices;
using AnotherECS.Core.Collection;

namespace AnotherECS.Core.Caller
{
    public static class AttachDetachActions
    {
        /*
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void RevertToSparseBuffer<TSparse>
        (uint tick, ref NArray<TSparse> subject, ref NArray<TOData<TSparse>> buffer, ref uint bufferIndex, ref NArray<BAllocator, TSparse> bufferCopy, ref NArray<BAllocator, Op> ops)
        where TSparse : unmanaged, IEquatable<TSparse>
        {
            var bufferPtr = buffer.GetPtr();
            var subjectPtr = subject.GetPtr();

            bufferCopy.Resize(subject.ByteLength);
            bufferCopy.CopyFrom(subject);

            var bufferCopyPtr = bufferCopy.GetPtr();
            TSparse zero = default;

            if (ops.Length != subject.Length)
            {
                ops.Resize(subject.Length);
            }
            else
            {
                ops.Clear();
            }

            if (bufferIndex != 0)
            {
                for (uint i = bufferIndex - 1; i >= 0; --i)
                {
                    var frame = bufferPtr[i];

                    if (frame.tick > tick)
                    {
                        ref var copy = ref bufferCopyPtr[frame.offset];
                        ref var op = ref ops.GetRef(frame.offset);

                        if (op != Op.BOTH)
                        {
                            if (!copy.Equals(frame.value))
                            {
                                op |= Op.BOTH;
                            }
                            else if (copy.Equals(zero) && !frame.value.Equals(zero))
                            {
                                op |= Op.ADD;
                            }
                            else if (!copy.Equals(zero) && frame.value.Equals(zero))
                            {
                                op = Op.REMOVE;
                            }
                        }

                        subjectPtr[frame.offset] = frame.value;
                    }
                    else
                    {
                        bufferIndex = i + 1;
                        return;
                    }
                }
            }


            for (uint i = buffer.Length - 1; i >= bufferIndex; --i)
            {
                var frame = bufferPtr[i];

                if (frame.tick > tick)
                {
                    ref var copy = ref bufferCopyPtr[frame.offset];
                    ref var op = ref ops.GetRef(frame.offset);

                    if (op != Op.BOTH)
                    {
                        if (!copy.Equals(frame.value))
                        {
                            op |= Op.BOTH;
                        }
                        else if (copy.Equals(zero) && !frame.value.Equals(zero))
                        {
                            op |= Op.ADD;
                        }
                        else if (!copy.Equals(zero) && frame.value.Equals(zero))
                        {
                            op = Op.REMOVE;
                        }
                    }

                    subjectPtr[frame.offset] = frame.value;
                }
                else
                {
                    bufferIndex = (i + 1) % buffer.Length;
                    return;
                }
            }
        }*/

        
        /*
     * [MethodImpl(MethodImplOptions.AggressiveInlining)]
public static unsafe void RevertTo<TSparse, TDense, TDenseIndex, TTickData, TAttachDetachStorage, TAttach, TDetach, TSparseBoolConst, TVersion, TIsUseSparse, KRevertDense>
(UnmanagedLayout<TSparse, TDense, TDenseIndex, TTickData>* layout, ref TAttachDetachStorage attachDetachStorage, uint tick)
where TSparse : unmanaged, IEquatable<TSparse>
where TDense : unmanaged
where TDenseIndex : unmanaged
where TTickData : unmanaged

where TAttachDetachStorage : struct, IAttachDetachProvider<TSparse>, IBoolConst
where TAttach : struct, IAttach<TSparse, TDense, TDenseIndex, TTickData>, IBoolConst
where TDetach : struct, IDetach<TSparse, TDense, TDenseIndex, TTickData>, IBoolConst
where TSparseBoolConst : struct, IBoolConst
where TVersion : struct, IBoolConst
where TIsUseSparse : struct, IUseSparse
where KRevertDense : struct, IDenseRevert<TSparse, TDense, TDenseIndex, TTickData>
{
RevertToValueBuffer(tick, ref layout->storage.denseIndex, ref layout->history.countBuffer, ref layout->history.countIndex);

RevertToValueBuffer(tick, ref layout->storage.recycleIndex, ref layout->history.recycleCountBuffer, ref layout->history.recycleCountIndex);
RevertToValueBuffer(tick, ref layout->storage.recycle, ref layout->history.recycleBuffer, ref layout->history.recycleIndex);

TIsUseSparse isUseSparse = default;
if (isUseSparse.IsUseSparse)
{
    if (attachDetachStorage.Is)
    {
        var bufferCopy = attachDetachStorage.GetSparseTempBuffer();
        var ops = attachDetachStorage.GetOps();
        RevertToSparseBuffer(tick, ref layout->storage.sparse, ref layout->history.sparseBuffer, ref layout->history.sparseIndex, ref bufferCopy, ref ops);

        TDetach detach = default;
        if (detach.Is)
        {
            detach.Detach<TSparseBoolConst>(layout, attachDetachStorage.GetState(), ref ops);
        }

        KRevertDense revertDense = default;
        revertDense.RevertDense(ref *layout, tick);


        TAttach attach = default;
        if (attach.Is)
        {
            attach.Attach<TSparseBoolConst>(layout, attachDetachStorage.GetState(), ref ops);
        }
    }
    else
    {
        RevertToValueBuffer(tick, ref layout->storage.sparse, ref layout->history.sparseBuffer, ref layout->history.sparseIndex);

        KRevertDense revertDense = default;
        revertDense.RevertDense(ref *layout, tick);
    }
}

TVersion version = default;
if (version.Is)
{
    LayoutActions.UpdateVersion(ref *layout, tick, layout->storage.denseIndex);
}
}
    */
    }
}

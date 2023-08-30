using System;
using System.Runtime.CompilerServices;

namespace AnotherECS.Core
{
    internal unsafe interface IRevertGetSparseRaw<U>
    {
        U GetSparseRaw();
    }

    internal unsafe interface IRevertSetSparseRaw<U>
        where U : unmanaged
    {
        void SetSparseRaw(U value);
    }

    internal static partial class RevertHelper
    {   
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RevertToSingleSparseBuffer<T>(ref T subject, uint tick, TickData<bool>[] sparseBuffer, ref int sparseIndex, out Op ops)
            where T : struct, IRevertGetSparseRaw<bool>, IRevertSetSparseRaw<bool>
        {
            var sparse = subject.GetSparseRaw();
            ops = 0;

            for (int i = sparseIndex - 1; i >= 0; --i)
            {
                var frame = sparseBuffer[i];

                if (frame.tick > tick)
                {
                    if (ops != Op.BOTH)
                    {
                        if (sparse != frame.value)
                        {
                            ops |= Op.BOTH;
                        }
                        else if (!sparse && frame.value)
                        {
                            ops |= Op.ADD;
                        }
                        else if (sparse && !frame.value)
                        {
                            ops = Op.REMOVE;
                        }
                    }

                    subject.SetSparseRaw(frame.value);
                }
                else
                {
                    sparseIndex = i + 1;
                    return;
                }
            }


            for (int i = sparseBuffer.Length - 1; i >= sparseIndex; --i)
            {
                var frame = sparseBuffer[i];

                if (frame.tick > tick)
                {
                    if (ops != Op.BOTH)
                    {
                        if (sparse != frame.value)
                        {
                            ops |= Op.BOTH;
                        }
                        else if (!sparse && frame.value)
                        {
                            ops |= Op.ADD;
                        }
                        else if (sparse && !frame.value)
                        {
                            ops = Op.REMOVE;
                        }
                    }

                    subject.SetSparseRaw(frame.value);
                }
                else
                {
                    sparseIndex = (i + 1) % sparseBuffer.Length;
                    return;
                }
            }

        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RevertToSingleSparseBuffer<T>(ref T subject, uint tick, TickData<bool>[] sparseBuffer, ref int sparseIndex)
            where T : struct, IRevertGetSparseRaw<bool>, IRevertSetSparseRaw<bool>
        {
            var isNeedContinueSearch = true;

            for (int i = sparseIndex - 1; i >= 0; --i)
            {
                if (sparseBuffer[i].tick <= tick)
                {
                    var lastIndex = i + 1;
                    if (i < sparseIndex)
                    {
                        subject.SetSparseRaw(sparseBuffer[lastIndex].value);
                        sparseIndex = lastIndex;
                        isNeedContinueSearch = false;
                    }
                    break;
                }
            }

            if (isNeedContinueSearch)
            {
                for (int i = sparseBuffer.Length - 1; i >= sparseIndex; --i)
                {
                    if (sparseBuffer[i].tick <= tick)
                    {
                        var lastIndex = i + 1;
                        if (lastIndex < sparseBuffer.Length)
                        {
                            subject.SetSparseRaw(sparseBuffer[lastIndex].value);
                            sparseIndex = lastIndex;
                            isNeedContinueSearch = false;
                        }
                        break;
                    }
                }
            }

            if (isNeedContinueSearch)
            {
                subject.SetSparseRaw(sparseBuffer[sparseIndex].value);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RevertToSparseBuffer<T, U>(ref T subject, uint tick, SparseData<U>[] sparseBuffer, ref int sparseIndex)
            where T : struct, IRevertGetSparseRaw<U[]>
            where U : unmanaged
        {
            var sparse = subject.GetSparseRaw();

            for (int i = sparseIndex - 1; i >= 0; --i)
            {
                var frame = sparseBuffer[i];

                if (frame.tick > tick)
                {
                    sparse[frame.sparseIndex] = frame.sparseValue;
                }
                else
                {
                    sparseIndex = i + 1;
                    return;
                }
            }

            for (int i = sparseBuffer.Length - 1; i >= sparseIndex; --i)
            {
                var frame = sparseBuffer[i];

                if (frame.tick > tick)
                {
                    sparse[frame.sparseIndex] = frame.sparseValue;
                }
                else
                {
                    sparseIndex = (i + 1) % sparseBuffer.Length;
                    return;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RevertToSparseBuffer<T>(ref T subject, uint tick, SparseData<bool>[] sparseBuffer, ref int sparseIndex, ref bool[] sparseCopy, ref Op[] bufferOps)
            where T : struct, IRevertGetSparseRaw<bool[]>
        {
            var sparse = subject.GetSparseRaw();

            if (sparseCopy.Length != sparse.Length)
            {
                sparseCopy = new bool[sparse.Length];
            }
            Array.Copy(sparse, sparseCopy, sparse.Length);

            if (bufferOps.Length != sparse.Length)
            {
                bufferOps = new Op[sparse.Length];
            }
            else
            {
                Array.Clear(bufferOps, 0, bufferOps.Length);
            }

            for (int i = sparseIndex - 1; i >= 0; --i)
            {
                var frame = sparseBuffer[i];

                if (frame.tick > tick)
                {
                    ref var copy = ref sparseCopy[frame.sparseIndex];
                    ref var op = ref bufferOps[frame.sparseIndex];

                    if (op != Op.BOTH)
                    {
                        if (copy != frame.sparseValue)
                        {
                            op |= Op.BOTH;
                        }
                        
                        else if (!copy && frame.sparseValue)
                        {
                            op |= Op.ADD;
                        }
                        else if (copy && !frame.sparseValue)
                        {
                            op = Op.REMOVE;
                        }
                    }

                    sparse[frame.sparseIndex] = frame.sparseValue;
                }
                else
                {
                    sparseIndex = i + 1;
                    return;
                }
            }


            for (int i = sparseBuffer.Length - 1; i >= sparseIndex; --i)
            {
                var frame = sparseBuffer[i];

                if (frame.tick > tick)
                {
                    ref var copy = ref sparseCopy[frame.sparseIndex];
                    ref var op = ref bufferOps[frame.sparseIndex];

                    if (op != Op.BOTH)
                    {
                        if (copy != frame.sparseValue)
                        {
                            op |= Op.BOTH;
                        }
                        else if (!copy && frame.sparseValue)
                        {
                            op |= Op.ADD;
                        }
                        else if (copy && !frame.sparseValue)
                        {
                            op = Op.REMOVE;
                        }
                    }

                    sparse[frame.sparseIndex] = frame.sparseValue;
                }
                else
                {
                    sparseIndex = (i + 1) % sparseBuffer.Length;
                    break;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RevertToSparseBuffer<T>(ref T subject, uint tick, SparseData<byte>[] sparseBuffer, ref int sparseIndex, ref byte[] sparseCopy, ref Op[] bufferOps)
            where T : struct, IRevertGetSparseRaw<byte[]>
        {
            var sparse = subject.GetSparseRaw();

            if (sparseCopy.Length != sparse.Length)
            {
                sparseCopy = new byte[sparse.Length];
            }
            Array.Copy(sparse, sparseCopy, sparse.Length);

            if (bufferOps.Length != sparse.Length)
            {
                bufferOps = new Op[sparse.Length];
            }
            else
            {
                Array.Clear(bufferOps, 0, bufferOps.Length);
            }

            for (int i = sparseIndex - 1; i >= 0; --i)
            {
                var frame = sparseBuffer[i];

                if (frame.tick > tick)
                {
                    ref var copy = ref sparseCopy[frame.sparseIndex];
                    ref var op = ref bufferOps[frame.sparseIndex];

                    if (op != Op.BOTH)
                    {
                        if (copy != frame.sparseValue)
                        {
                            op |= Op.BOTH;
                        }
                        else if (copy == 0 && frame.sparseValue != 0)
                        {
                            op |= Op.ADD;
                        }
                        else if (copy != 0 && frame.sparseValue == 0)
                        {
                            op = Op.REMOVE;
                        }
                    }

                    sparse[frame.sparseIndex] = frame.sparseValue;
                }
                else
                {
                    sparseIndex = i + 1;
                    return;
                }
            }


            for (int i = sparseBuffer.Length - 1; i >= sparseIndex; --i)
            {
                var frame = sparseBuffer[i];

                if (frame.tick > tick)
                {
                    ref var copy = ref sparseCopy[frame.sparseIndex];
                    ref var op = ref bufferOps[frame.sparseIndex];

                    if (op != Op.BOTH)
                    {
                        if (copy != frame.sparseValue)
                        {
                            op |= Op.BOTH;
                        }
                        else if (copy == 0 && frame.sparseValue != 0)
                        {
                            op |= Op.ADD;
                        }
                        else if (copy != 0 && frame.sparseValue == 0)
                        {
                            op = Op.REMOVE;
                        }
                    }

                    sparse[frame.sparseIndex] = frame.sparseValue;
                }
                else
                {
                    sparseIndex = (i + 1) % sparseBuffer.Length;
                    break;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RevertToSparseBuffer<T>(ref T subject, uint tick, SparseData<ushort>[] sparseBuffer, ref int sparseIndex, ref ushort[] sparseCopy, ref Op[] bufferOps)
            where T : struct, IRevertGetSparseRaw<ushort[]>
        {
            var sparse = subject.GetSparseRaw();

            if (sparseCopy.Length != sparse.Length)
            {
                sparseCopy = new ushort[sparse.Length];
            }
            Array.Copy(sparse, sparseCopy, sparse.Length);

            if (bufferOps.Length != sparse.Length)
            {
                bufferOps = new Op[sparse.Length];
            }
            else
            {
                Array.Clear(bufferOps, 0, bufferOps.Length);
            }

            for (int i = sparseIndex - 1; i >= 0; --i)
            {
                var frame = sparseBuffer[i];

                if (frame.tick > tick)
                {
                    ref var copy = ref sparseCopy[frame.sparseIndex];
                    ref var op = ref bufferOps[frame.sparseIndex];

                    if (op != Op.BOTH)
                    {
                        if (copy != frame.sparseValue)
                        {
                            op |= Op.BOTH;
                        }
                        else if (copy == 0 && frame.sparseValue != 0)
                        {
                            op |= Op.ADD;
                        }
                        else if (copy != 0 && frame.sparseValue == 0)
                        {
                            op = Op.REMOVE;
                        }
                    }

                    sparse[frame.sparseIndex] = frame.sparseValue;
                }
                else
                {
                    sparseIndex = i + 1;
                    return;
                }
            }


            for (int i = sparseBuffer.Length - 1; i >= sparseIndex; --i)
            {
                var frame = sparseBuffer[i];

                if (frame.tick > tick)
                {
                    ref var copy = ref sparseCopy[frame.sparseIndex];
                    ref var op = ref bufferOps[frame.sparseIndex];

                    if (op != Op.BOTH)
                    {
                        if (copy != frame.sparseValue)
                        {
                            op |= Op.BOTH;
                        }
                        else if (copy == 0 && frame.sparseValue != 0)
                        {
                            op |= Op.ADD;
                        }
                        else if (copy != 0 && frame.sparseValue == 0)
                        {
                            op = Op.REMOVE;
                        }
                    }

                    sparse[frame.sparseIndex] = frame.sparseValue;
                }
                else
                {
                    sparseIndex = (i + 1) % sparseBuffer.Length;
                    break;
                }
            }

        }
    }
}
using System.Runtime.CompilerServices;

namespace AnotherECS.Core
{
    internal interface IRevertSetRecycledCountRaw<U>
        where U : unmanaged
    {
        void SetRecycledCountRaw(U value);
    }

    internal interface IRevertGetRecycledRaw<U>
        where U : unmanaged
    {
        U[] GetRecycledRaw();
    }

    internal static partial class RevertHelper
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RevertToRecycledCountBuffer<T, U>(ref T subject, uint tick, TickData<U>[] recycledCountBuffer, ref int recycledCountIndex)
            where T : struct, IRevertSetRecycledCountRaw<U>
            where U : unmanaged
        {
            for (int i = recycledCountIndex - 1; i >= 0; --i)
            {
                var frame = recycledCountBuffer[i];

                if (frame.tick > tick)
                {
                    subject.SetRecycledCountRaw(frame.value);
                }
                else
                {
                    recycledCountIndex = i + 1;
                    return;
                }
            }

            for (int i = recycledCountBuffer.Length - 1; i >= recycledCountIndex; --i)
            {
                var frame = recycledCountBuffer[i];

                if (frame.tick > tick)
                {
                    subject.SetRecycledCountRaw(frame.value);
                }
                else
                {
                    recycledCountIndex = (i + 1) % recycledCountBuffer.Length;
                    return;
                }
            }
        }
        /*
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RevertToRecycledBuffer<T>(ref T subject, uint tick, RecycleData<ushort>[] recycledBuffer, ref int recycledIndex)
           where T : struct, IRevertGetRecycledRaw<ushort>
        {
            var isNeedContinueSearch = true;

            var recycled = subject.GetRecycledRaw();

            for (int i = recycledIndex - 1; i >= 0; --i)
            {
                var frame = recycledBuffer[i];

                if (frame.tick > tick)
                {
                    recycled[frame.index] = frame.value;
                }
                else
                {
                    recycledIndex = i + 1;
                    isNeedContinueSearch = false;
                    break;
                }
            }

            if (isNeedContinueSearch)
            {
                for (int i = recycledBuffer.Length - 1; i >= recycledIndex; --i)
                {
                    var frame = recycledBuffer[i];

                    if (frame.tick > tick)
                    {
                        recycled[frame.index] = frame.value;
                    }
                    else
                    {
                        recycledIndex = (i + 1) % recycledBuffer.Length;
                        break;
                    }
                }
            }
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RevertToRecycledBuffer<T>(ref T subject, uint tick, RecycleData<uint>[] recycledBuffer, ref int recycledIndex)
            where T : struct, IRevertGetRecycledRaw<uint>
        {
            var isNeedContinueSearch = true;

            var recycled = subject.GetRecycledRaw();

            for (int i = recycledIndex - 1; i >= 0; --i)
            {
                var frame = recycledBuffer[i];

                if (frame.tick > tick)
                {
                    recycled[frame.index] = frame.value;
                }
                else
                {
                    recycledIndex = i + 1;
                    isNeedContinueSearch = false;
                    break;
                }
            }

            if (isNeedContinueSearch)
            {
                for (int i = recycledBuffer.Length - 1; i >= recycledIndex; --i)
                {
                    var frame = recycledBuffer[i];

                    if (frame.tick > tick)
                    {
                        recycled[frame.index] = frame.value;
                    }
                    else
                    {
                        recycledIndex = (i + 1) % recycledBuffer.Length;
                        break;
                    }
                }
            }
        }
        */
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RevertToRecycledCountBufferClass<T, U>(T subject, uint tick, TickData<U>[] recycledCountBuffer, ref int recycledCountIndex)
           where T : class, IRevertSetRecycledCountRaw<U>
           where U : unmanaged
        {
            for (int i = recycledCountIndex - 1; i >= 0; --i)
            {
                var frame = recycledCountBuffer[i];

                if (frame.tick > tick)
                {
                    subject.SetRecycledCountRaw(frame.value);
                }
                else
                {
                    recycledCountIndex = i + 1;
                    return;
                }
            }

            for (int i = recycledCountBuffer.Length - 1; i >= recycledCountIndex; --i)
            {
                var frame = recycledCountBuffer[i];

                if (frame.tick > tick)
                {
                    subject.SetRecycledCountRaw(frame.value);
                }
                else
                {
                    recycledCountIndex = (i + 1) % recycledCountBuffer.Length;
                    return;
                }
            }
        }
        /*
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RevertToRecycledBufferClass<T>(T subject, uint tick, RecycleData<ushort>[] recycledBuffer, ref int recycledIndex)
            where T : class, IRevertGetRecycledRaw<ushort>
        {
            var recycled = subject.GetRecycledRaw();

            for (int i = recycledIndex - 1; i >= 0; --i)
            {
                var frame = recycledBuffer[i];

                if (frame.tick > tick)
                {
                    recycled[frame.index] = frame.value;
                }
                else
                {
                    recycledIndex = i + 1;
                    return;
                }
            }

            for (int i = recycledBuffer.Length - 1; i >= recycledIndex; --i)
            {
                var frame = recycledBuffer[i];

                if (frame.tick > tick)
                {
                    recycled[frame.index] = frame.value;
                }
                else
                {
                    recycledIndex = (i + 1) % recycledBuffer.Length;
                    return;
                }
            }
        }*/
    }
}
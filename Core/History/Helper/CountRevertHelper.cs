using System.Runtime.CompilerServices;

namespace AnotherECS.Core
{
    internal interface IRevertSetCountRaw<U>
        where U : unmanaged
    {
        void SetCountRaw(U value);
    }
    
    internal static partial class RevertHelper
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RevertToCountBuffer<T, U>(ref T subject, uint tick, TickData<U>[] countBuffer, ref int countIndex)
            where T : struct, IRevertSetCountRaw<U>
            where U : unmanaged
        {

            for (int i = countIndex - 1; i >= 0; --i)
            {
                var frame = countBuffer[i];

                if (frame.tick > tick)
                {
                    subject.SetCountRaw(frame.value);
                }
                else
                {
                    countIndex = i + 1;
                    return;
                }
            }

            for (int i = countBuffer.Length - 1; i >= countIndex; --i)
            {
                var frame = countBuffer[i];

                if (frame.tick > tick)
                {
                    subject.SetCountRaw(frame.value);
                }
                else
                {
                    countIndex = (i + 1) % countBuffer.Length;
                    return;
                }
            }
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RevertToCountBufferClass<T, U>(T subject, uint tick, TickData<U>[] countBuffer, ref int countIndex)
            where T : class, IRevertSetCountRaw<U>
            where U : unmanaged
        {
            for (int i = countIndex - 1; i >= 0; --i)
            {
                var frame = countBuffer[i];

                if (frame.tick > tick)
                {
                    subject.SetCountRaw(frame.value);
                }
                else
                {
                    countIndex = i + 1;
                    return;
                }
            }

            for (int i = countBuffer.Length - 1; i >= countIndex; --i)
            {
                var frame = countBuffer[i];

                if (frame.tick > tick)
                {
                    subject.SetCountRaw(frame.value);
                }
                else
                {
                    countIndex = (i + 1) % countBuffer.Length;
                    return;
                }
            }
        }

    }
}
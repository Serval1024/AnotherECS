using System.Runtime.CompilerServices;

namespace AnotherECS.Core
{
    internal unsafe interface IRevertPtrDenseRaw
    {
        byte* GetDenseRaw();
    }

    internal unsafe interface IRevertSetSingleDenseRaw<U>
        where U : struct
    {
        void SetSingleDenseRaw(ref U value);
    }

    internal static partial class RevertHelper
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void RevertToElementBuffer<T, U>(ref T subject, uint tick, ElementOffsetData<U>[] elementUshortBuffer, ref int elementUshortIndex)
            where T : struct, IRevertPtrDenseRaw
            where U : unmanaged
        {
            var elements = (U*)subject.GetDenseRaw();

            for (int i = elementUshortIndex - 1; i >= 0; --i)
            {
                var frame = elementUshortBuffer[i];

                if (frame.tick > tick)
                {
                    elements[frame.offset] = frame.data;
                }
                else
                {
                    elementUshortIndex = i + 1;
                    return;
                }
            }

            for (int i = elementUshortBuffer.Length - 1; i >= elementUshortIndex; --i)
            {
                var frame = elementUshortBuffer[i];

                if (frame.tick > tick)
                {
                    elements[frame.offset] = frame.data;
                }
                else
                {
                    elementUshortIndex = (i + 1) % elementUshortBuffer.Length;
                    return;
                }
            }
        }


        public static void RevertToSingleComponentBuffer<T, U>(ref T subject, uint tick, ComponentData<U>[] componentBuffer, ref int componentIndex)
            where T : struct, IRevertSetSingleDenseRaw<U>
            where U : struct
        {
            var isNeedContinueSearch = true;

            for (int i = componentIndex - 1; i >= 0; --i)
            {
                if (componentBuffer[i].tick <= tick)
                {
                    var lastIndex = i + 1;
                    if (i < componentIndex)
                    {
                        subject.SetSingleDenseRaw(ref componentBuffer[lastIndex].component);
                        componentIndex = lastIndex;
                        isNeedContinueSearch = false;
                    }
                    break;
                }
            }

            if (isNeedContinueSearch)
            {
                for (int i = componentBuffer.Length - 1; i >= componentIndex; --i)
                {
                    if (componentBuffer[i].tick <= tick)
                    {
                        var lastIndex = i + 1;
                        if (lastIndex < componentBuffer.Length)
                        {
                            subject.SetSingleDenseRaw(ref componentBuffer[lastIndex].component);
                            componentIndex = lastIndex;
                            isNeedContinueSearch = false;
                        }
                        break;
                    }
                }
            }

            if (isNeedContinueSearch)
            {
                subject.SetSingleDenseRaw(ref componentBuffer[componentIndex].component);
            }
        }
    }
}
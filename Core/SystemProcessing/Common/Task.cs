using System.Runtime.CompilerServices;

namespace AnotherECS.Core.Threading
{
    internal struct Task<THandler, TData> : ITask
        where THandler : struct, ITaskHandler<TData>
        where TData : struct
    {
        public TData arg;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Invoke()
        {
            default(THandler).Invoke(ref arg);
        }
    }
}
using System.Runtime.CompilerServices;

namespace AnotherECS.Core.Collection
{
    public interface IIterator<TData>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Each(ref TData data);
    }
}
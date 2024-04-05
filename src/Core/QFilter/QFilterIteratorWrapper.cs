using AnotherECS.Core.Caller;
using AnotherECS.Core.Collection;
using System.Runtime.CompilerServices;

namespace AnotherECS.Core
{
    internal struct QFilterIteratorWrapper<TComponent, TIterator> : IDataIterator<TComponent>
           where TIterator : struct, IIterator<TComponent>
           where TComponent : unmanaged
    {
        public TIterator data;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Each(uint index, ref TComponent component)
        {
            data.Each(ref component);
        }
    }
}

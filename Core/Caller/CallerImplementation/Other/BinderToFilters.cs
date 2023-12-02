using System.Runtime.CompilerServices;

namespace AnotherECS.Core.Caller
{
    internal unsafe struct BinderToFilters : IBinderToFilters
    {
        public bool IsTemporary { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => false; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(ref GlobalDepencies depencies, uint id, ushort elementId)
        {
            depencies.filters.Add(id, elementId, false);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Remove(ref GlobalDepencies depencies, uint id, ushort elementId)
        {
            depencies.filters.Remove(id, elementId);
        }
    }

    internal unsafe struct TempBinderToFilters : IBinderToFilters
    {
        public bool IsTemporary { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => true; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(ref GlobalDepencies depencies, uint id, ushort elementId)
        {
            depencies.filters.Add(id, elementId, true);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Remove(ref GlobalDepencies depencies, uint id, ushort elementId)
        {
            depencies.filters.Remove(id, elementId);
        }
    }
}

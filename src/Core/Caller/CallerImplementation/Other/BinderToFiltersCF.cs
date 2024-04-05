using System.Runtime.CompilerServices;

namespace AnotherECS.Core.Caller
{
    internal unsafe struct BinderToFiltersCF : IBinderToFilters
    {
        public bool IsTemporary { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => false; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(ref Dependencies dependencies, uint id, uint elementId)
        {
            dependencies.filters.Add(id, elementId, false);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Remove(ref Dependencies dependencies, uint id, uint elementId)
        {
            dependencies.filters.Remove(id, elementId);
        }
    }

    internal unsafe struct TempBinderToFiltersCF : IBinderToFilters
    {
        public bool IsTemporary { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => true; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(ref Dependencies dependencies, uint id, uint elementId)
        {
            dependencies.filters.Add(id, elementId, true);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Remove(ref Dependencies dependencies, uint id, uint elementId)
        {
            dependencies.filters.Remove(id, elementId);
        }
    }
}

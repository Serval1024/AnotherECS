using System.Runtime.CompilerServices;

namespace AnotherECS.Core.Caller
{
    internal unsafe struct BinderToFilters : IBinderToFilters
    {
        public bool IsTemporary { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => false; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(ref GlobalDependencies dependencies, uint id, ushort elementId)
        {
            dependencies.filters.Add(id, elementId, false);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Remove(ref GlobalDependencies dependencies, uint id, ushort elementId)
        {
            dependencies.filters.Remove(id, elementId);
        }
    }

    internal unsafe struct TempBinderToFilters : IBinderToFilters
    {
        public bool IsTemporary { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => true; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(ref GlobalDependencies dependencies, uint id, ushort elementId)
        {
            dependencies.filters.Add(id, elementId, true);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Remove(ref GlobalDependencies dependencies, uint id, ushort elementId)
        {
            dependencies.filters.Remove(id, elementId);
        }
    }
}

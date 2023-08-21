namespace AnotherECS.Core
{
    [ComponentOption(ComponentOptions.NoCompileDirectAccess | ComponentOptions.ExceptSparseDirectDense | ComponentOptions.CompileSortAtLast | ComponentOptions.ReferencePool)]
    public unsafe struct EntityData : IComponent
    {
        public ushort generation;
        public ushort count;
        public fixed ushort components[16];
    }
}

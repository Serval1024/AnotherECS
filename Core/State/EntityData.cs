namespace AnotherECS.Core
{
    [ComponentOption(ComponentOptions.NoCompileDirectAccess | ComponentOptions.CompileSortAtLast)]
    public unsafe struct EntityData : IComponent
    {
        public ushort generation;
        public ushort count;
        public fixed ushort components[16];
    }
}

namespace AnotherECS.Core.Caller
{
    internal unsafe struct ComponentFunctionData<TDense>
        where TDense : unmanaged
    {
        public Dependencies* dependencies;
        public ComponentFunction<TDense> componentFunction;
    }
}
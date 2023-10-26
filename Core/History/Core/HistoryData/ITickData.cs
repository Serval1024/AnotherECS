namespace AnotherECS.Core
{
    internal interface ITickData<TDense>
    {
        public uint Tick { get; }
        public TDense Value { get; }
    }
}

namespace AnotherECS.Collections
{
    public interface IFString
    {
        public uint Capacity { get; }
        public uint Length { get; }
    }

    public interface IFString<TData> : IFString
        where TData : struct
    {
        TData this[uint index] { get; set; }
    }
}
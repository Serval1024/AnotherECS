namespace AnotherECS.Collections
{
    public interface ICString
    {
        public uint Capacity { get; }
        public uint Length { get; }
    }

    public interface ICString<TData> : ICString
        where TData : struct
    {
        TData this[uint index] { get; set; }
    }
}
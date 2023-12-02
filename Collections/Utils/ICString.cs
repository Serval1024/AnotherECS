namespace AnotherECS.Collections
{
    public interface ICString
    {
        public int Capacity { get; }
        public int Length { get; }
    }

    public interface ICString<TData> : ICString
        where TData : struct
    {
        TData this[int index] { get; set; }
    }
}
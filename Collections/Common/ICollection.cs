namespace AnotherECS.Collections
{
    public interface ICollection
    {
        uint Count { get; }
        object Get(uint index);
        void Set(uint index, object value);
        void Clear();
    }

    public interface ICollection<TData> : ICollection
       where TData : struct
    {
        TData this[uint index] { get; set; }
    }
}
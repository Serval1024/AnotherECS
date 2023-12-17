namespace AnotherECS.Collections
{
    public interface ICArray
    {
        uint Length { get; }
        object Get(uint index);
        void Set(uint index, object value);
        void Clear();
    }

    public interface ICArray<TData> : ICArray
       where TData : struct
    {
        TData this[uint index] { get; set; }
    }
}
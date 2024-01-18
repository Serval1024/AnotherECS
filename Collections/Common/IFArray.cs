namespace AnotherECS.Collections
{
    public interface IFArray
    {
        uint Length { get; }
        object Get(uint index);
        void Set(uint index, object value);
        void Clear();
    }

    public interface IFArray<TData> : IFArray
       where TData : struct
    {
        TData this[uint index] { get; set; }
    }
}
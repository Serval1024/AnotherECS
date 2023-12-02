namespace AnotherECS.Collections
{
    public interface ICArray
    {
        int Length { get; }
        object Get(int index);
        void Set(int index, object value);
        void Clear();
    }

    public interface ICArray<TData> : ICArray
       where TData : struct
    {
        TData this[int index] { get; set; }
    }
}
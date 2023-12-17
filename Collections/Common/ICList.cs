namespace AnotherECS.Collections
{
    public interface ICList
    {
        uint Count { get; }
        uint Capacity { get; }
        object Get(uint index);
        void Set(uint index, object value);
        void Add(object value);
        void ExtendToCapacity();
        void RemoveAt(uint index);
        void RemoveLast();
        void Clear();
    }

    public interface ICList<TData> : ICList
       where TData : struct
    {
        TData this[uint index] { get; set; }
        void Add(TData value);
    }
}
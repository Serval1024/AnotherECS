namespace AnotherECS.Collections
{
    public interface ICList
    {
        int Count { get; }
        int Capacity { get; }
        object Get(int index);
        void Set(int index, object value);
        void Add(object value);
        void ExtendToCapacity();
        void RemoveAt(int index);
        void RemoveLast();
        void Clear();
    }

    public interface ICList<TData> : ICList
       where TData : struct
    {
        TData this[int index] { get; set; }
        void Add(TData value);
    }
}
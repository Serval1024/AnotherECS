namespace AnotherECS.Collections
{
    public interface IListCollection : ICollection
    {
        uint Capacity { get; }
        void Add(object value);
        void ExtendToCapacity();
        void RemoveAt(uint index);
        void RemoveLast();
    }

    public interface IListCollection<TData> : IListCollection, ICollection<TData>
       where TData : struct
    {
        void Add(TData value);
    }
}
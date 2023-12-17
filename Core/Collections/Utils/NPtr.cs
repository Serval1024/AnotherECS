namespace AnotherECS.Core.Collection
{
    public unsafe struct NPtr<T>
        where T : unmanaged
    {
        public T* Value { get; private set; }
        public bool IsValide => Value != null;

        public NPtr(T* ptr)
        {
            Value = ptr;
        }
    }

}

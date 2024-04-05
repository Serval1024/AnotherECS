using System.Runtime.CompilerServices;

namespace AnotherECS.Core.Collection
{
    public unsafe struct WPtr<T>
        where T : unmanaged
    {
        public T* Value
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)] 
            get;
            private set;
        }

        public bool IsValid => Value != null;

        public WPtr(T* ptr)
        {
            Value = ptr;
        }
    }

}

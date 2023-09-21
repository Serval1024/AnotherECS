using AnotherECS.Core.Actions;
using System;

namespace AnotherECS.Core
{
    public unsafe struct SimpleCaller<TComponent> : IMultiCaller<TComponent>
       where TComponent : unmanaged, IComponent
    {
        private UnmanagedLayout<TComponent>* _layout;
        private GlobalDepencies* _depencies;

        public void Add(uint id, ref TComponent component)
        {
            throw new NotImplementedException();
        }

        public ref TComponent Add(uint id)
        {
            throw new NotImplementedException();
        }

        public TComponent Create()
        {
            throw new NotImplementedException();
        }

        public ref TComponent Get(uint id)
        {
            throw new NotImplementedException();
        }

        public IComponent GetCopy(uint id)
        {
            throw new NotImplementedException();
        }

        public Type GetElementType()
            => default;
        

        public bool IsHas(uint id)
        {
            throw new NotImplementedException();
        }

        public ref TComponent Read(uint id)
        {
            throw new NotImplementedException();
        }

        public void Remove(uint id)
        {
            throw new NotImplementedException();
        }

        public ref TComponent Set(uint id, ref TComponent component)
        {
            throw new NotImplementedException();
        }

        public void Set(uint id, IComponent component)
        {
            throw new NotImplementedException();
        }

        void IMultiCaller<TComponent>.Add(uint id, ref TComponent component)
        {
            throw new NotImplementedException();
        }

        ref TComponent IMultiCaller<TComponent>.Add(uint id)
        {
            throw new NotImplementedException();
        }

        void ICaller.AllocateLayout()
        {
            //StorageActions<TComponent>.AllocateLayout<bool>(ref *_layout, _depencies->config.entityCapacity, _depencies->config.componentCapacity, _depencies->config.recycledCapacity);
        }

        void ICaller.Config(UnmanagedLayout* layout, GlobalDepencies* depencies, ushort id, State state)
        {
            throw new NotImplementedException();
        }

        TComponent ICaller<TComponent>.Create()
        {
            throw new NotImplementedException();
        }

        ref TComponent IMultiCaller<TComponent>.Get(uint id)
        {
            throw new NotImplementedException();
        }

        IComponent IMultiCaller.GetCopy(uint id)
        {
            throw new NotImplementedException();
        }

        Type ICaller.GetElementType()
        {
            throw new NotImplementedException();
        }

        bool IMultiCaller<TComponent>.IsHas(uint id)
        {
            throw new NotImplementedException();
        }

        ref TComponent IMultiCaller<TComponent>.Read(uint id)
        {
            throw new NotImplementedException();
        }

        void IMultiCaller.Remove(uint id)
        {
            throw new NotImplementedException();
        }

        ref TComponent IMultiCaller<TComponent>.Set(uint id, ref TComponent component)
        {
            throw new NotImplementedException();
        }

        void IMultiCaller.Set(uint id, IComponent component)
        {
            throw new NotImplementedException();
        }
    }

}


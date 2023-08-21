using System;
using EntityId = System.Int32;

namespace AnotherECS.Core
{
    public interface IPool
    {
        public Type GetElementType();
        void Clear();
    }

    public interface IEntityPool : IPool
    {
        public bool IsHas(EntityId id);
        void Resize(int capacity);
        void Remove(EntityId id);
    }

    public interface ISinglePool : IPool
    {
        public bool IsHas();
        void Remove();
    }

    public interface IComponentFactory<T>
        where T : IComponent
    {
        T Create();
    }

    public interface IInjectSupport
    {
        void BindInject(ref InjectContainer injectContainer, IInjectMethodsReference[] injectMethods);
        void ReInject();
    }
}


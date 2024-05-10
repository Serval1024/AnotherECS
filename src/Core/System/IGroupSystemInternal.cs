using System;
using System.Collections.Generic;

namespace AnotherECS.Core
{
    internal interface IGroupSystemInternal : IGroupSystem, IFeature
    {
        bool IsHas(Type type);
        void Prepend(ISystem system);
        void Append(ISystem system);
        void Sort(ISystemRegister systemRegister);
    }

    public interface IGroupSystem : IDisposable, ISystem, IEnumerable<ISystem>
    {
        IEnumerable<ISystem> GetSystemsAll();
    }
}
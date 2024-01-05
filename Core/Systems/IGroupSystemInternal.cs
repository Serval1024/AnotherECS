using System;
using System.Collections.Generic;

namespace AnotherECS.Core
{
    internal interface IGroupSystemInternal : IGroupSystem
    {
        void Prepend(ISystem system);
        void Sort();
    }

    public interface IGroupSystem : IDisposable, ISystem, IEnumerable<ISystem>
    {
        IEnumerable<ISystem> GetSystemsAll();
    }
}
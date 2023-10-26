using System;
using EntityId = System.UInt32;

namespace AnotherECS.Core
{
   

    public interface IInjectSupport
    {
        void BindInject(ref InjectContainer injectContainer, IInjectMethodsReference[] injectMethods);
        void ReInject();
    }
}


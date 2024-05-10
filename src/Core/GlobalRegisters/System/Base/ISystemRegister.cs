using System;
using System.Collections.Generic;

namespace AnotherECS.Core
{
    public interface ISystemRegister
    {
        Dictionary<Type, int> GetOrders();
    }
}



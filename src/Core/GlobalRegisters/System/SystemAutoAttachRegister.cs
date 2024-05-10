using System;
using System.Collections.Generic;
using System.Linq;

namespace AnotherECS.Core
{
    public class SystemAutoAttachRegister : ISystemAutoAttachRegister
    {
        private readonly HashSet<Type> _data = new();

        public void Install<T>()
            where T : ISystem
        {
            lock (_data)
            {
                if (!_data.Contains(typeof(T)))
                {
                    _data.Add(typeof(T));
                }
            }
        }

        public Type[] Gets()
        {
            lock (_data)
            {
                return _data.ToArray();
            }
        }
    }
}



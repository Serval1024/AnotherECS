using System;

namespace AnotherECS.Core
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = true)]
    public class SystemOrderAttribute : Attribute
    {
        public SystemOrderRelative OrderRelative { get; private set; }
        public SystemOrder Order { get; private set; }
        public Type System { get; private set; }

        public SystemOrderAttribute(SystemOrderRelative order, Type system)
        {
            if (!typeof(ISystem).IsAssignableFrom(system))
            {
                throw new ArgumentException($"Type '{system.Name}' must be inherited from '{nameof(ISystem)}'.");
            }

            OrderRelative = order;
            System = system;
        }

        public SystemOrderAttribute(SystemOrder order)
        {
            Order = order;
        }
    }

    public enum SystemOrderRelative
    {
        None,
        Before,
        After,
    }

    public enum SystemOrder
    {
        None,
        First,
        Last,
    }
}
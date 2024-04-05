using System;

namespace AnotherECS.Serializer
{
    [AttributeUsage(AttributeTargets.Struct | AttributeTargets.Class | AttributeTargets.Interface, Inherited = true)]
    public class SerializeAttribute : Attribute { }
}

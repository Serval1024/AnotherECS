using System;

namespace AnotherECS.Serializer
{
    [AttributeUsage(AttributeTargets.Struct | AttributeTargets.Class | AttributeTargets.Interface)]
    public class SerializeAttribute : Attribute { }
}

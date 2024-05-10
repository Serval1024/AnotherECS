using System;
using System.Collections.Generic;

namespace AnotherECS.Core.Caller
{
    public struct GenericDeclaration
    {
        public Type Type;
        public List<GenericDeclaration> Generic;

        public GenericDeclaration(Type type)
            : this(type, new List<GenericDeclaration>())
        { }

        public GenericDeclaration(Type type, List<GenericDeclaration> generic)
        {
            Type = type;
            Generic = generic;
        }
    }
}

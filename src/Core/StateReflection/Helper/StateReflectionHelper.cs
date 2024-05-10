using AnotherECS.Core.Caller;
using System;

namespace AnotherECS.Core
{
    internal static class StateReflectionHelper
    {
        public static ICallerReference CreateCallerByDeclaration(in GenericDeclaration declaration)
            => CreateCallerByDeclaration(MakeTypeByDeclaration(declaration));

        public static ICallerReference CreateCallerByDeclaration(Type type)
            => Activator.CreateInstance(type) as ICallerReference;

        public static Type MakeTypeByDeclaration(in GenericDeclaration declaration)
        {
            if (declaration.Generic.Count != 0)
            {
                var genericTypes = new Type[declaration.Generic.Count];
                for(int i = 0; i < declaration.Generic.Count; ++i)
                {
                    genericTypes[i] = MakeTypeByDeclaration(declaration.Generic[i]);
                }

                return declaration.Type.MakeGenericType(genericTypes);
            }
            else
            {
                return declaration.Type;
            }
        }
    }
}

using System;
using System.Linq;

namespace AnotherECS.Unity.Debug.Diagnostic
{
    public static class TypeUtils
    {  
        public static Type[] GetAllowIsAssignableFromTypesAcrossAll<T>()
            where T : class
            => AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(domainAssembly => domainAssembly.GetTypes())
                .Where(p => typeof(T).IsAssignableFrom(p))
                .ToArray();

        public static Type[] GetRuntimeTypes<T>()
           where T : class
           => GetAllowIsAssignableFromTypesAcrossAll<T>()
               .Where(p => !p.IsInterface)
               .ToArray();
    }
}


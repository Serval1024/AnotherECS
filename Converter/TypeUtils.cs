using System;
using System.Linq;
using System.Reflection;

namespace AnotherECS.Converter
{
    public static class TypeUtils
    {
        public static Type[] GetOfTypeDerivedFromAcrossAll<T>()
            where T : class
            => AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(p => p.GetTypes())
                .Where(myType => myType.IsClass && !myType.IsAbstract && myType.IsSubclassOf(typeof(T)))
                .ToArray();

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

        public static Type[] GetAllowHasAttributeFromTypesAcrossAll<T>()
           where T : Attribute
           => AppDomain.CurrentDomain.GetAssemblies()
               .SelectMany(domainAssembly => domainAssembly.GetTypes())
               .Where(
                   p => p.GetCustomAttribute(typeof(T), false) != null
                   )
               .ToArray();

        public static Type FindType(string fullName)
            => AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(domainAssembly => domainAssembly.GetTypes())
                .First(p => p.FullName == fullName);
    }
}


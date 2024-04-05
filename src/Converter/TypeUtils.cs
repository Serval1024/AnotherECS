using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace AnotherECS.Converter
{
    public static class TypeUtils
    {
        public static IEnumerable<Type> GetOfTypeDerivedFromAcrossAll<T>()
            where T : class
            => AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(p => p.GetTypes())
                .Where(myType => myType.IsClass && !myType.IsAbstract && myType.IsSubclassOf(typeof(T)));

        public static IEnumerable<Type> GetAllowIsAssignableFromTypesAcrossAll<T>()
            where T : class
            => AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(domainAssembly => domainAssembly.GetTypes())
                .Where(p => typeof(T).IsAssignableFrom(p));

        public static IEnumerable<Type> GetRuntimeTypes<T>()
            where T : class
            => GetAllowIsAssignableFromTypesAcrossAll<T>()
                .Where(p => !p.IsInterface);

        public static IEnumerable<Type> GetAllowHasAttributeFromTypesAcrossAll<T>()
           where T : Attribute
           => AppDomain.CurrentDomain.GetAssemblies()
               .SelectMany(domainAssembly => domainAssembly.GetTypes())
               .Where(
                   p => p.GetCustomAttribute(typeof(T), true) != null || p.GetInterfaces().Any(p0 => p0.GetCustomAttribute(typeof(T)) != null)
                   );

        public static Type FindType(string fullName)
            => AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(domainAssembly => domainAssembly.GetTypes())
                .First(p => p.FullName == fullName);
    }
}


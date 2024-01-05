using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using AnotherECS.Core.Collection;

[assembly: InternalsVisibleTo("AnotherECS.Generator")]
namespace AnotherECS.Core
{
    public static class ReflectionUtils
    {
        public static Type[] ExtractGenericFromInterface<T>(Type type)
        {
            var interfaces = type.GetInterfaces().Where(p => p.Name.StartsWith($"{typeof(T).Name}`"));

            return interfaces.Any()
                ? interfaces
                    .Select(p => p.GetGenericArguments())
                    .SelectMany(p => p)
                    .GroupBy(p => p)
                    .Select(p => p.First())
                    .ToArray()
                : Array.Empty<Type>();
        }

        public static string GetUnderLineName(Type type)
            => type.Name
            .Replace('+', '_')
            .Replace('.', '_');

        public static string GetGenericFullName(Type type, Dictionary<Type, Type> map = null)
            =>
            type.IsGenericType
                ? $"{type.SwapGenericToName(map)[..type.SwapGenericToName(map).IndexOf('`')]}<{string.Join(", ", type.GetGenericArguments().Select(p => GetDotFullName(p, map)))}>"
                : type.SwapGenericToName(map);

        public static string GetUnderLineFullName(Type type, Dictionary<Type, Type> map = null)
            => GetGenericFullName(type, map)
            .Replace('+', '_')
            .Replace('.', '_');

        public static string GetDotFullName(Type type, Dictionary<Type, Type> map = null)
            => GetGenericFullName(type, map)
            .Replace('+', '.');

        public static void ReflectionRebindMemoryHandle<T>(ref T component, ref MemoryRebinderContext rebinder)
            where T : struct
        {
            var methodName = nameof(IRebindMemoryHandle.RebindMemoryHandle);
            var args = new object[] { rebinder };
            if (typeof(IRebindMemoryHandle).IsAssignableFrom(typeof(T)))
            {
                var boxing = (IRebindMemoryHandle)component;
                var method = GetMethod(typeof(T), methodName);
                method.Invoke(boxing, args);
                component = (T)boxing;
            }

            foreach (var member in
                typeof(T)
                .GetFieldsAndProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(p => typeof(IInject).IsAssignableFrom(p.GetMemberType()))
                )
            {
                var memberOfComponent = member.GetValue(component);
                var method = GetMethod(member.GetMemberType(), methodName);

                method.Invoke(memberOfComponent, args);

                object copy = component;
                member.SetValue(copy, memberOfComponent);
                component = (T)copy;
            }
        }

        public static void ReflectionInjectConstruct<T>(ref T component, ref InjectContainer injectContainer)
            where T : struct
            => ReflectionInject(ref component, ref injectContainer, nameof(IInject<WPtr<HAllocator>>.Construct));

        public static void ReflectionInjectDeconstruct<T>(ref T component, ref InjectContainer injectContainer)
            where T : struct
            => ReflectionInject(ref component, ref injectContainer, nameof(IInject.Deconstruct));

        public static void ReflectionInject<T>(ref T component, ref InjectContainer injectContainer, string methodName)
            where T : struct
        {
            if (typeof(IInject).IsAssignableFrom(typeof(T)))
            {
                var boxing = (IInject)component;
                var method = GetMethod(typeof(T), methodName);
                var args = GetArgs(typeof(T), ref injectContainer);
                method.Invoke(boxing, args);
                component = (T)boxing;
            }

            foreach (var member in
                typeof(T)
                .GetFieldsAndProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(p => typeof(IInject).IsAssignableFrom(p.GetMemberType()))
                )
            {
                var memberOfComponent = member.GetValue(component);
                var method = GetMethod(member.GetMemberType(), methodName);
                var args = GetArgs(member.GetMemberType(), ref injectContainer);

                method.Invoke(memberOfComponent, args);

                object copy = component;
                member.SetValue(copy, memberOfComponent);
                component = (T)copy;
            }



            static object[] GetArgs(Type type, ref InjectContainer injectContainer)
            {
                var requirementTypes = ExtractGenericFromInterface<IInject>(type);
                var injectTypes = injectContainer.GetType().GetProperties();
                var args = new object[requirementTypes.Length];
                for (var i = 0; i < args.Length; ++i)
                {
                    args[i] = injectTypes.First(p => p.PropertyType == requirementTypes[i]).GetValue(injectContainer);
                }
                return args;
            }
        }

        public static string GetMemberName(this MemberInfo memberInfo)
            => memberInfo.MemberType switch
            {
                MemberTypes.Field => ((FieldInfo)memberInfo).Name,
                MemberTypes.Property => ((PropertyInfo)memberInfo).Name,
                _ => throw new NotImplementedException(),
            };

        public static Type GetMemberType(this MemberInfo memberInfo)
            => memberInfo.MemberType switch
            {
                MemberTypes.Field => ((FieldInfo)memberInfo).FieldType,
                MemberTypes.Property => ((PropertyInfo)memberInfo).PropertyType,
                _ => throw new NotImplementedException(),
            };

        public static object GetValue(this MemberInfo memberInfo, object instance)
            => memberInfo.MemberType switch
            {
                MemberTypes.Field => ((FieldInfo)memberInfo).GetValue(instance),
                MemberTypes.Property => ((PropertyInfo)memberInfo).GetValue(instance),
                _ => throw new NotImplementedException(),
            };

        public static void SetValue(this MemberInfo memberInfo, object instance, object value)
        {
            switch (memberInfo.MemberType)
            {
                case MemberTypes.Field:
                    ((FieldInfo)memberInfo).SetValue(instance, value);
                    break;
                case MemberTypes.Property:
                    ((PropertyInfo)memberInfo).SetValue(instance, value);
                    break;
                default:
                    throw new NotImplementedException();
            };
        }

        private static MethodInfo GetMethod(Type type, string methodName)
              => type.GetInterfaces().Select(p => p.GetMethod(methodName)).First(p => p != null);

        public static IEnumerable<MemberInfo> GetFieldsAndProperties(this Type type, BindingFlags bindingFlags)
            => type
                .GetFields(bindingFlags)
                .Cast<MemberInfo>()
                .Union(type.GetProperties(bindingFlags));

        public static string GetNameWithoutGeneric(this Type type)
        {
            string name = type.Name;
            int index = name.IndexOf('`');
            return index == -1 ? name : name[..index];
        }

        private static string SwapGenericToName(this Type type, Dictionary<Type, Type> map = null)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            if (type.FullName == null)
            {
                var realCandidateTypes = FindTypeFromTemplateType(type);

                if (map != null)
                {
                    foreach (var realCandidateType in realCandidateTypes)
                    {
                        if (map.TryGetValue(realCandidateType, out var mapType))
                        {
                            return GetDotFullName(mapType);
                        }
                    }
                }

                return GetDotFullName(realCandidateTypes.First());
            }

            return type.FullName;
        }

        private static IEnumerable<Type> FindTypeFromTemplateType(Type type)
        {
            var interfaces = type.GetInterfaces();

            return (type.BaseType != null)
                ? interfaces.Append(type.BaseType) 
                : interfaces;
        }
    }
}
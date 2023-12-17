using AnotherECS.Core.Caller;
using AnotherECS.Serializer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace AnotherECS.Core
{
    internal static class ComponentUtils
    {
        private const BindingFlags DATA_FREE_FLAGS =
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        public static bool IsOption(Type type, ComponentOptions option)
        {
            var attribute = type.GetCustomAttribute<CompileComponentOptionAttribute>();
            return (attribute != null) && attribute.Options.HasFlag(option);
        }

        public static bool IsVersion(Type type)
            => typeof(IVersion).IsAssignableFrom(type);

        public static bool IsConfig(Type type)
            => typeof(IConfig).IsAssignableFrom(type);

        public static bool IsHistory(Type type)
            => !IsOption(type, ComponentOptions.HistoryNonSync);

        public static bool IsUnmanaged(Type type)
        {
            if (type.IsPrimitive || type.IsPointer || type.IsEnum)
            {
                return true;
            }

            if (!type.IsValueType)
            {
                return false;
            }

            return type
                .GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                .All(p => IsUnmanaged(p.FieldType));
        }

        public static bool IsSimple(Type type)
        {
            if (type.IsPrimitive || type.IsEnum)
            {
                return true;
            }

            if (!type.IsValueType)
            {
                return false;
            }

            return type
                .GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                .All(p => IsSimple(p.FieldType));
        }

        public static bool IsBlittable(Type type)
        {
            if (type.IsPrimitive || type.IsEnum || type.IsPointer || type.GetCustomAttribute<ForceBlittableAttribute>() != null)
            {
                return true;
            }

            if (!type.IsValueType)
            {
                return false;
            }

            return type
                .GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                .All(p => IsBlittable(p.FieldType));
        }

        public static bool IsAttach(Type type)
            => typeof(IAttach).IsAssignableFrom(type);

        public static bool IsDetach(Type type)
            => typeof(IDetach).IsAssignableFrom(type);

        public static bool IsDefault(Type type)
            => typeof(IDefault).IsAssignableFrom(type);

        public static bool IsSingle(Type type)
            => typeof(ISingle).IsAssignableFrom(type);

        public static bool IsMarker(Type type)
            => typeof(IMarker).IsAssignableFrom(type);

        public static bool IsEmpty(Type type)
            => (IsOption(type, ComponentOptions.DataFree) || (type.GetFields(DATA_FREE_FLAGS).Length == 0 && type.GetProperties(DATA_FREE_FLAGS).Length == 0))
            && !IsOption(type, ComponentOptions.NotDataFree);

        public static bool IsWithoutSparseDirectDense(Type type)
            => IsOption(type, ComponentOptions.WithoutSparseDirectDense) || GetTypeSize(type) <= 2;

        public static bool IsCompileFastAccess(Type type)
            => IsOption(type, ComponentOptions.CompileFastAccess);

        public static bool IsInjectComponent(Type type)
            => typeof(IInject).IsAssignableFrom(type);

        public static bool IsUseISerialize(Type type)
            => IsOption(type, ComponentOptions.UseISerialize);

        public static bool IsInjectMembers(Type type)
            => type.GetFieldsAndProperties(DATA_FREE_FLAGS)
            .Any(p => typeof(IInject).IsAssignableFrom(p.GetMemberType()));

        public static bool IsRebindMemory(Type type)
            => typeof(IRebindMemoryHandle).IsAssignableFrom(type);

        public static bool IsRebindMemoryMembers(Type type)
            => type.GetFieldsAndProperties(DATA_FREE_FLAGS)
            .Any(p => typeof(IRebindMemoryHandle).IsAssignableFrom(p.GetMemberType()));

        public static int GetTypeSize(Type type)            
            => System.Runtime.InteropServices.Marshal.SizeOf(type);

        public static FieldData[] GetFieldToMembers<T>(Type type)
        {
            var result = new List<FieldData>();

            foreach (var member in type.GetFieldsAndProperties(DATA_FREE_FLAGS))
            {
                if (typeof(IInject).IsAssignableFrom(member.GetMemberType()))
                {
                    result.Add(new FieldData
                    {
                        fieldName = member.GetMemberName(),
                        argumentTypes = ReflectionUtils.ExtractGenericFromInterface<T>(member.GetMemberType()).ToArray(),
                    });
                }
            }

            return result.ToArray();
        }


        public struct FieldData
        {
            public string fieldName;
            public Type[] argumentTypes;
        }
    }
}

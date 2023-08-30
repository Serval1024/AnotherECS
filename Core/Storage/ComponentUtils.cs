using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace AnotherECS.Core
{
    internal static class ComponentUtils
    {
        private const BindingFlags DATA_FREE_FLAGS = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        public static bool IsOption(Type type, ComponentOptions option)
        {
            var attribute = type.GetCustomAttribute<ComponentOptionAttribute>();
            return (attribute != null) && attribute.Options.HasFlag(option);
        }

        public static int GetOption(Type type, ComponentOptions option)
        {
            var attribute = type.GetCustomAttribute<ComponentOptionAttribute>();
            if ((attribute != null) && attribute.Options.HasFlag(option))
            {
                if (option == ComponentOptions.Capacity)
                {
                    return attribute.Capacity;
                }
            }
            return 0;
        }

        public static bool IsCopyable(Type type)
            => typeof(ICopyable).IsAssignableFrom(type);

        public static bool IsBlittable(Type type)
            => IsUnmanaged(type) || IsOption(type, ComponentOptions.Blittable);

        public static bool IsVersion(Type type)
            => typeof(IVersion).IsAssignableFrom(type);

        public static bool IsHistory(Type type)
#if ANOTHERECS_HISTORY_DISABLE
            => false;
#else
            => !IsOption(type, ComponentOptions.HistoryNonSync);
#endif

        public static bool IsHistoryByChange(Type type)
            => IsHistory(type) && !IsHistoryByTick(type);

        public static bool IsHistoryByTick(Type type)
            => IsHistory(type) && IsOption(type, ComponentOptions.HistoryByTick);

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
                .All(f => IsUnmanaged(f.FieldType));
        }

        public static bool IsAttach(Type type)
            => typeof(IAttach).IsAssignableFrom(type);

        public static bool IsDetach(Type type)
            => typeof(IDetach).IsAssignableFrom(type);

        public static bool IsShared(Type type)
            => typeof(IShared).IsAssignableFrom(type);

        public static bool IsMarker(Type type)
            => typeof(IMarker).IsAssignableFrom(type);

        public static bool IsLimit255(Type type)
            => IsOption(type, ComponentOptions.StorageLimit255);

        public static bool IsEmpty(Type type)
            => (IsOption(type, ComponentOptions.DataFree) || (type.GetFields(DATA_FREE_FLAGS).Length == 0 && type.GetProperties(DATA_FREE_FLAGS).Length == 0))
            && !IsOption(type, ComponentOptions.DataNotFree);

        public static bool IsExceptSparseDirectDense(Type type)
            => IsOption(type, ComponentOptions.ExceptSparseDirectDense) || GetTypeSize(type) <= 2;

        public static bool IsCompileDirectAccess(Type type)
            => !IsOption(type, ComponentOptions.NoCompileDirectAccess);

        public static bool IsSortAtLast(Type type)
            => IsOption(type, ComponentOptions.CompileSortAtLast) && !IsCompileDirectAccess(type);

        public static bool IsOverrideCapacity(Type type)
            => IsOption(type, ComponentOptions.Capacity);

        public static int GetOverrideCapacity(Type type)
            => GetOption(type, ComponentOptions.Capacity);

        public static bool IsInjectComponent(Type type)
            => typeof(IInject).IsAssignableFrom(type);

        public static bool IsForceUseISerialize(Type type)
            => IsOption(type, ComponentOptions.ForceUseISerialize);

        public static bool IsInjectMembers(Type type)
            => type.GetFieldsAndProperties(DATA_FREE_FLAGS)
            .Any(p => typeof(IInject).IsAssignableFrom(p.GetMemberType()));

        public static bool IsReferenceStorage(Type type)
            => IsOption(type, ComponentOptions.ReferenceStorage);

        public static int GetTypeSize(Type type)            
            => System.Runtime.InteropServices.Marshal.SizeOf(type);

        public static InjectData[] GetInjectToMembers(Type type)
        {
            var result = new List<InjectData>();

            foreach (var field in type.GetFields(DATA_FREE_FLAGS))
            {
                if (typeof(IInject).IsAssignableFrom(field.FieldType))
                {
                    result.Add(new InjectData
                    {
                        fieldName = field.Name,
                        argumentTypes = ReflectionUtils.ExtractGenericFromInterface<IInject>(field.FieldType).Select(p => p.Name).ToArray(),
                    });
                }
            }
            foreach (var field in type.GetProperties(DATA_FREE_FLAGS))
            {
                result.Add(new InjectData
                {
                    fieldName = field.Name,
                    argumentTypes = ReflectionUtils.ExtractGenericFromInterface<IInject>(field.PropertyType).Select(p => p.Name).ToArray(),
                });
            }

            return result.ToArray();
        }


        public struct InjectData
        {
            public string fieldName;
            public string[] argumentTypes;
        }
    }
}


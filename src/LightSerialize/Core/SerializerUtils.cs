using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace AnotherECS.Serializer
{
    public static class SerializerUtils
    {
        private readonly static Dictionary<Type, MemberInfo[]> _memberInfoCache = new();

        public static MemberInfo[] GetMembers(Type type)
        {
            if (_memberInfoCache.TryGetValue(type, out MemberInfo[] result))
            {
                return result;
            }
            else
            {
                var memberInfos = type
                    .GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                    .OrderBy(p => p.Name).ToArray();

                _memberInfoCache.Add(type, memberInfos);
                return memberInfos;
            }
        }

        public static object GetValue(MemberInfo memberInfo, object instance)
           => ReflectionUtils.GetValue(memberInfo, instance);

        public static void SetValue(MemberInfo memberInfo, object instance, object value)
            => ReflectionUtils.SetValue(memberInfo, instance, value);

        public static unsafe ulong EnumAsUInt64<T>(T value)
            where T : unmanaged, Enum
        {
            ulong result;
            if (sizeof(T) == 1)
                result = *(byte*)(&value);
            else if (sizeof(T) == 2)
                result = *(ushort*)(&value);
            else if (sizeof(T) == 4)
                result = *(uint*)(&value);
            else if (sizeof(T) == 8)
                result = *(ulong*)(&value);
            else
                throw new ArgumentException("Argument is not a usual enum type; it is not 1, 2, 4, or 8 bytes in length.");
            return result;
        }
        public unsafe static T UInt64AsEnum<T>(ulong value)
            where T : unmanaged, Enum
            => *(T*)&value;
    }
}

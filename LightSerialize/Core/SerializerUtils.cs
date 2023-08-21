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
    }
}

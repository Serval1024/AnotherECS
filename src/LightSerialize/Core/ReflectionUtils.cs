using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("AnotherECS.Generator")]
namespace AnotherECS.Serializer
{
    public static class ReflectionUtils
    {
        public static IEnumerable<MemberInfo> GetFieldsAndProperties(this Type type, BindingFlags bindingFlags)
            => type
                .GetFields(bindingFlags)
                .Cast<MemberInfo>()
                .Union(type.GetProperties(bindingFlags));

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
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AnotherECS.Converter;
using AnotherECS.Core;
using Unity.Collections;

namespace AnotherECS.Generator
{
    internal static class VariablesConfigGenerator
    {
        public static TemplateParser.Variables GetStorage(TypeOptions option)
            => new()
            {
                { "GENERIC_CONSTRAINTS:TComponent", p => TypeOptionsUtils.GetStorageInterfaces(option) },
                { "STORAGE:TYPE_NAME", p => TypeOptionsUtils.GetStorageFlags(option) },

                { "ATTACH", p => option.isAttach },
                { "DETACH", p => option.isDetach },
                { "SINGLE", p => option.isSingle },
                { "MULTI", p => !option.isSingle },
                { "STORAGE:MODE", p => option.isSingle ? "Single" : "Multi" },
                { "EMPTY", p => option.isEmpty },
                { "ALLOCATOR:RECYCLE", p => option.isUseRecycle },
                { "INJECT", p => option.isInject },
                { "SPARSE:BOOL", p => option.sparseMode == TypeOptions.SparseMode.Bool },
                { "HISTORY", p => option.isHistory },
                { "HISTORY:BYCHANGE", p => option.isHistoryByChange },
                { "HISTORY:BYTICK", p => option.isHistoryByTick },
                { "BIND_TO_ENTITY", p => option.isBindToEntity   },
                { "COPYABLE", p => option.isCopyable },
                { "SPARSE:TYPE_NAME", p => option.sparseMode.ToString().ToLower() },
                { "FORCE:ISerialize", p => option.isUseISerialize },
                
                { "CALLER:TYPE_TYPE", p => TypeOptionsUtils.GetCallerFlags(option) },
            };

        public static TemplateParser.Variables GetLayoutInstaller(Type[] components)
        {
            TemplateParser.Variables variables = null;
            variables = new()
                {
                    { "COMPONENT:COUNT", p => components.Length.ToString() },
                    { "COMPONENT:NAME", p => components[p].Name },
                    { "COMPONENT:FULL_NAME", p => components[p].FullName },
                    { "CALLER:NAME", p => TypeOptionsUtils.GetCallerFlags(new TypeOptions(components[p])) },

                    { "INJECT", p => new TypeOptions(components[p]).isInject },
                    { "INJECT:SELF", p => new TypeOptions(components[p]).isInjectComponent },
                    { "INJECT:ARGS_SELF", p => GetInjectArguments(components[p]) },

                    { "INJECT:COUNT", p => new TypeOptions(components[variables.GetIndex(0)]).injectMembers.Length.ToString() },
                    { "INJECT:FIELD_NAME", p => new TypeOptions(components[variables.GetIndex(0)]).injectMembers[variables.GetIndex(1)].fieldName },
                    { "INJECT:ARGS", p => GetInjectArguments(new TypeOptions(components[variables.GetIndex(0)]).injectMembers[variables.GetIndex(1)].argumentTypes) },
                };
            return variables;
        }

        public static TemplateParser.Variables GetState(Type stateType, string stateGenName, ITypeToUshort components)
        {
            TemplateParser.Variables variables = null;
            variables = new()
                {
                    { "STATE:NAME", p => stateType.Name },
                    { "STATE:GEN_NAME", p => stateGenName },

                    { "COMPONENT:FASTACCESS", p => new TypeOptions(components.IdToType(p)).isCompileFastAccess },
                    { "COMPONENT:COUNT", p => components.GetAssociationTable().Count.ToString() },
                    { "COMPONENT:FULL_NAME", p => components.IdToType(p).FullName },
                };
            return variables;
        }

        public static TemplateParser.Variables GetSystem(ITypeToUshort states, ITypeToUshort systems)
        {
            TemplateParser.Variables variables = null;
            variables = new()
            {
                { "STATE:COUNT", p => states.Count().ToString() },
                { "STATE:GEN_NAME", p => StateGenerator.GetStateNameGen(states.IdToType(variables.GetIndexAsId(0))) },

                { "SYSTEM:COUNT", p => systems.Count().ToString() },
                { "SYSTEM:NAME", p => GetSystemName(states, systems, variables.GetIndexAsId(0), variables.GetIndexAsId(1)) },
            };
            return variables;
        }

        private static string GetSystemName(ITypeToUshort states, ITypeToUshort systems, ushort id0, ushort id1)
        {
            try
            {
                var state = states.IdToType(id0);
                var typeMapTemp = new Dictionary<Type, Type>
                {
                    { typeof(IState), state },
                    { typeof(State), state }
                };

                return ReflectionUtils.GetGeneratorFullName(systems.IdToType(id1), typeMapTemp);
            }
            catch (InvalidOperationException e)
            {
                throw new Exception("Unable to sort generic system.", e);
            }
        }

        private static string GetInjectArguments(Type type)
           => GetInjectArguments(
               ReflectionUtils.ExtractGenericFromInterface<IInject>(type)
               .Select(p => p.Name)
               .ToArray()
               );

        private static string GetInjectArguments(string[] types)
        {
            var result = new StringBuilder();

            for (int i = 0; i < types.Length; ++i)
            {
                result.Append($"injectContainer.{types[i]}");
                if (i < types.Length - 1)
                {
                    result.Append(",");
                }
            }
            return result.ToString();
        }
    }
}

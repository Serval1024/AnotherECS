using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AnotherECS.Converter;
using AnotherECS.Core;
using AnotherECS.Core.Actions;

namespace AnotherECS.Generator
{
    internal static class VariablesConfigGenerator
    {
        public static TemplateParser.Variables GetCaller(TypeOptions option)
            => new()
            {
                { "GENERIC_CONSTRAINTS:TComponent", () => TypeOptionsUtils.GetCallerInterfaces(option) },
                { "CALLER:TYPE_NAME", () => TypeOptionsUtils.GetCallerFlags(option) },

                { "ATTACH", () => option.isAttach },
                { "DETACH", () => option.isDetach },
                { "SINGLE", () => option.isSingle },
                { "MULTI", () => !option.isSingle },
                { "STORAGE:MODE", () => option.isSingle ? "Single" : "Multi" },
                { "EMPTY", () => option.isEmpty },
                { "ALLOCATOR:RECYCLE", () => option.isUseRecycle },
                { "INJECT", () => option.isInject },
                { "SPARSE:BOOL", () => option.sparseMode == TypeOptions.SparseMode.Bool },
                { "HISTORY", () => option.isHistory },
                { "HISTORY:BYCHANGE", () => option.isHistoryByChange },
                { "HISTORY:BYTICK", () => option.isHistoryByTick },
                { "BIND_TO_ENTITY", () => option.isBindToEntity   },
                { "COPYABLE", () => option.isCopyable },
                { "SPARSE:TYPE_NAME", () => option.sparseMode.ToString().ToLower() },
                { "FORCE:ISerialize", () => option.isUseISerialize },

                { "HISTORY:FLAG", () =>  GetHistoryFlag(option) },
            };

        public static TemplateParser.Variables GetLayoutInstaller(Type[] components)
        {
            TemplateParser.Variables variables = null;
            variables = new()
                {
                    { "COMPONENT:COUNT", () => components.Length.ToString() },
                    { "COMPONENT:NAME", () => components[variables.GetIndex(0)].Name },
                    { "COMPONENT:FULL_NAME", () => ReflectionUtils.GetDotFullName(components[variables.GetIndex(0)]) },
                    { "COMPONENT:FULL_NAME_AS_TEXT", () => ReflectionUtils.GetUnderLineFullName(components[variables.GetIndex(0)]) },
                    { "CALLER:TYPE_NAME", () => TypeOptionsUtils.GetCallerFlags(new TypeOptions(components[variables.GetIndex(0)])) },

                    { "INJECT", () => new TypeOptions(components[variables.GetIndex(0)]).isInject },
                    { "INJECT:SELF", () => new TypeOptions(components[variables.GetIndex(0)]).isInjectComponent },
                    { "INJECT:SELF:ARGS", () => GetInjectArguments(components[variables.GetIndex(0)]) },

                    { "INJECT:FIELD:COUNT", () => new TypeOptions(components[variables.GetIndex(0)]).injectMembers.Length.ToString() },
                    { "INJECT:FIELD:NAME", () => new TypeOptions(components[variables.GetIndex(0)]).injectMembers[variables.GetIndex(1)].fieldName },
                    { "INJECT:FIELD:ARGS", () => GetInjectArguments(new TypeOptions(components[variables.GetIndex(0)]).injectMembers[variables.GetIndex(1)].argumentTypes) },
                };
            return variables;
        }

        public static TemplateParser.Variables GetState(Type stateType, string stateGenName, ITypeToUshort components)
        {
            var fastAccessComponents = new CustomTypeToIdConverter<ushort, IComponent>(
                components.GetAssociationTable().Values.Where(p => new TypeOptions(p).isCompileFastAccess)
                );

            TemplateParser.Variables variables = null;
            variables = new()
                {
                    { "STATE:NAME", () => stateType.Name },
                    { "STATE:GEN_NAME", () => stateGenName },

                    { "COMPONENT:COUNT", () => components.GetAssociationTable().Count.ToString() },
                    { "COMPONENT:FULL_NAME", () => ReflectionUtils.GetDotFullName(components.IdToType(variables.GetIndexAsId(0))) },
                    { "COMPONENT:FULL_NAME_AS_TEXT", () => ReflectionUtils.GetUnderLineFullName(components.IdToType(variables.GetIndexAsId(0))) },

                    { "CALLER:FASTACCESS:TYPE_NAME", () => TypeOptionsUtils.GetCallerFlags(new TypeOptions(fastAccessComponents.IdToType(variables.GetIndexAsId(0)))) },

                    { "COMPONENT:FASTACCESS", () => new TypeOptions(components.IdToType(variables.GetIndexAsId(0))).isCompileFastAccess },
                    { "COMPONENT:FASTACCESS:COUNT", () => fastAccessComponents.GetAssociationTable().Count.ToString() },
                    { "COMPONENT:FASTACCESS:NAME", () => ReflectionUtils.GetUnderLineName(fastAccessComponents.IdToType(variables.GetIndexAsId(0))) },
                    { "COMPONENT:FASTACCESS:FULL_NAME", () => ReflectionUtils.GetDotFullName(fastAccessComponents.IdToType(variables.GetIndexAsId(0))) },
                };
            return variables;
        }

        public static TemplateParser.Variables GetSystem(ITypeToUshort states, ITypeToUshort systems)
        {
            TemplateParser.Variables variables = null;
            variables = new()
            {
                { "STATE:COUNT", () => states.Count().ToString() },
                { "STATE:GEN_NAME", () => StateGenerator.GetStateNameGen(states.IdToType(variables.GetIndexAsId(0))) },

                { "SYSTEM:COUNT", () => systems.Count().ToString() },
                { "SYSTEM:NAME", () => GetSystemName(states, systems, variables.GetIndexAsId(0), variables.GetIndexAsId(1)) },
            };
            return variables;
        }

        public static TemplateParser.Variables GetComponent(GeneratorContext context)
        {
            var states = context.GetStates();
            TemplateParser.Variables variables = null;
            variables = new()
            {
                { "STATE:COUNT", () => states.Count().ToString() },
                { "STATE:GEN_NAME", () => StateGenerator.GetStateNameGen(states.IdToType(variables.GetIndexAsId(0))) },

                { "COMPONENT:COUNT", () =>
                    context.GetComponents(
                        states.IdToType(variables.GetIndexAsId(0))
                    ).Count().ToString()
                },
                { "COMPONENT:FULL_NAME", () => ReflectionUtils.GetDotFullName(
                    context.GetComponents(
                        states.IdToType(variables.GetIndexAsId(0))
                        )
                    .IdToType(variables.GetIndexAsId(1)))
                },
            };
            return variables;
        }

        private static string GetHistoryFlag(TypeOptions option)
            => nameof(HistoryMode) + "." +
            (option.isHistory
                ? (option.isHistoryByChange ? HistoryMode.BYCHANGE : HistoryMode.BYTICK)
                : HistoryMode.NONE).ToString();

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

                return ReflectionUtils.GetDotFullName(systems.IdToType(id1), typeMapTemp);
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

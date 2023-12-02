using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AnotherECS.Converter;
using AnotherECS.Core;

namespace AnotherECS.Generator
{
    internal static class VariablesConfigGenerator
    {
        public static TemplateParser.Variables GetCommonLayoutInstaller(Type[] components)
        {
            var callers = components
                .Select(p => new TypeOptions(p))
                .Select(p => (typeOption : p, name : TypeOptionsGeneratorUtils.GetCallerFlags(p)))
                .GroupBy(p => p.name).Select(p => p.First())
                .Select(p => (p.typeOption, p.name, declaration : TypeOptionsGeneratorUtils.GetLayoutDeclaration(p.typeOption)))
                .ToArray();

            TemplateParser.Variables variables = null;
            variables = new()
                {
                    { "CALLER:COUNT", () => callers.Length.ToString() },
                    { "CALLER:TYPE_NAME", () => callers[variables.GetIndex(0)].name },
                    { "GENERIC_CONSTRAINTS:TComponent", () => TypeOptionsGeneratorUtils.GetCallerInterfaces(callers[variables.GetIndex(0)].typeOption) },

                    { "CALLER:DECLARE", () => GetCallerDeclaration(callers[variables.GetIndex(0)].typeOption)},

                    { "LAYOUT:TSparse", () => callers[variables.GetIndex(0)].declaration.TSparse },
                    { "LAYOUT:TDenseIndex", () => callers[variables.GetIndex(0)].declaration.TDenseIndex },
                    { "LAYOUT:TTickData", () => callers[variables.GetIndex(0)].declaration.TTickData },

                    { "INJECT", () => callers[variables.GetIndex(0)].typeOption.isInject },

                };
            return variables;
        }

        private static string GetCallerDeclaration(TypeOptions option)
        {
            var result = TypeOptionsGeneratorUtils.GetCallerDeclaration(option);
            result.Append(Environment.NewLine);
            result.Append(new string('\t', 3));
            return result.ToString();
        }



        public static TemplateParser.Variables GetLayoutInstaller(Type[] components)
        {
            TemplateParser.Variables variables = null;
            variables = new()
                {
                    { "COMPONENT:COUNT", () => components.Length.ToString() },
                    { "COMPONENT:NAME", () => components[variables.GetIndex(0)].Name },
                    { "COMPONENT:FULL_NAME", () => ReflectionUtils.GetDotFullName(components[variables.GetIndex(0)]) },
                    { "COMPONENT:FULL_NAME_AS_TEXT", () => ReflectionUtils.GetUnderLineFullName(components[variables.GetIndex(0)]) },

                    { "CALLER:TYPE_NAME", () => TypeOptionsGeneratorUtils.GetCallerFlags(new TypeOptions(components[variables.GetIndex(0)])) },

                    { "INJECT", () => new TypeOptions(components[variables.GetIndex(0)]).isInject },
                    { "INJECT:SELF", () => new TypeOptions(components[variables.GetIndex(0)]).isInjectComponent },
                    { "INJECT:SELF:ARGS", () => GetInjectArguments(components[variables.GetIndex(0)]) },

                    { "INJECT:FIELD:COUNT", () => new TypeOptions(components[variables.GetIndex(0)]).injectMembers.Length.ToString() },
                    { "INJECT:FIELD:NAME", () => new TypeOptions(components[variables.GetIndex(0)]).injectMembers[variables.GetIndex(1)].fieldName },
                    { "INJECT:FIELD:ARGS", () => GetInjectArguments(new TypeOptions(components[variables.GetIndex(0)]).injectMembers[variables.GetIndex(1)].argumentTypes) },
                };
            return variables;
        }

        public static TemplateParser.Variables GetState(GeneratorContext context, string stateGenName, ITypeToUshort components)
        {
            var fastAccessComponents = new CustomTypeToIdConverter<ushort, IComponent>(
                components.GetAssociationTable().Values.Where(p => new TypeOptions(p).isCompileFastAccess)
                );

            TemplateParser.Variables variables = null;
            variables = new()
                {
                    { "STATE:GEN_NAME", () => stateGenName },

                    { "COMPONENT:COUNT", () => components.GetAssociationTable().Count.ToString() },
                    { "COMPONENT:FULL_NAME", () => ReflectionUtils.GetDotFullName(components.IdToType(variables.GetIndexAsId(0))) },
                    { "COMPONENT:FULL_NAME_AS_TEXT", () => ReflectionUtils.GetUnderLineFullName(components.IdToType(variables.GetIndexAsId(0))) },

                    { "CALLER:FASTACCESS:TYPE_NAME", () => TypeOptionsGeneratorUtils.GetCallerFlags(new TypeOptions(fastAccessComponents.IdToType(variables.GetIndexAsId(0)))) },

                    { "COMPONENT:FASTACCESS", () => new TypeOptions(components.IdToType(variables.GetIndexAsId(0))).isCompileFastAccess },
                    { "COMPONENT:FASTACCESS:COUNT", () => fastAccessComponents.GetAssociationTable().Count.ToString() },
                    { "COMPONENT:FASTACCESS:NAME", () => ReflectionUtils.GetUnderLineName(fastAccessComponents.IdToType(variables.GetIndexAsId(0))) },
                    { "COMPONENT:FASTACCESS:FULL_NAME", () => ReflectionUtils.GetDotFullName(fastAccessComponents.IdToType(variables.GetIndexAsId(0))) },

                    { "EMBENED:ComponentInstallerGenerator.cs", () => new ComponentInstallerGenerator().Compile(context, false).First().text },
                    { "EMBENED:SystemInstallerGenerator.cs", () => new SystemInstallerGenerator().Compile(context, false).First().text },
                };
            return variables;
        }

        public static TemplateParser.Variables GetSystem(ITypeToUshort states, ITypeToUshort systems)
        {
            TemplateParser.Variables variables = null;
            variables = new()
            {
                { "STATE:COUNT", () => states.Count().ToString() },
                { "STATE:GEN_NAME", () => states.IdToType(variables.GetIndexAsId(0)).Name },

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
                { "STATE:GEN_NAME", () => states.IdToType(variables.GetIndexAsId(0)).Name },

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

        public static TemplateParser.Variables GetFastAccess(TypeOptions option)
        {
            TemplateParser.Variables variables = null;
            variables = new()
                {
                    { "MULTI", () => !option.isSingle },
                    { "CALLER:TYPE_NAME", () => TypeOptionsGeneratorUtils.GetCallerFlags(option) },
                    { "CALLER:DECLARE", () => GetCallerDeclaration(option)},
                    { "GENERIC_CONSTRAINTS:TComponent", () => TypeOptionsGeneratorUtils.GetCallerInterfaces(option) },
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

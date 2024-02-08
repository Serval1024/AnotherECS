using System;
using System.Linq;
using System.Reflection;
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

                    { "LAYOUT:TAllocator", () => callers[variables.GetIndex(0)].declaration.TAllocator },
                    { "LAYOUT:TSparse", () => callers[variables.GetIndex(0)].declaration.TSparse },
                    { "LAYOUT:TDenseIndex", () => callers[variables.GetIndex(0)].declaration.TDenseIndex },

                    { "COMPONENT_FUNCTION", () =>
                        {
                            var typeOption = callers[variables.GetIndex(0)].typeOption;
                            return typeOption.isInject || typeOption.isRepairMemory || typeOption.isRepairStateId;
                        }
                    },

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



        public static TemplateParser.Variables GetLayoutInstaller(Type[] componentTypes)
        {
            var components = componentTypes.Select(p => new TypeOptions(p)).ToArray();
            var injectContext = InjectContext.Create();

            TemplateParser.Variables variables = null;
            variables = new()
                {
                    { "COMPONENT:COUNT", () => components.Length.ToString() },
                    { "COMPONENT:NAME", () => components[variables.GetIndex(0)].type.Name },
                    { "COMPONENT:FULL_NAME", () => ReflectionUtils.GetDotFullName(components[variables.GetIndex(0)].type) },
                    { "COMPONENT:FULL_NAME_AS_TEXT", () => ReflectionUtils.GetUnderLineFullName(components[variables.GetIndex(0)].type) },

                    { "CALLER:TYPE_NAME", () => TypeOptionsGeneratorUtils.GetCallerFlags(new TypeOptions(components[variables.GetIndex(0)].type)) },

                    { "INJECT", () => components[variables.GetIndex(0)].isInject },
                    { "INJECT:SELF", () => components[variables.GetIndex(0)].isInjectComponent },
                    { "INJECT:SELF:ARGS", () =>
                        {
                            InjectContextUtils.PrepareContext(ref injectContext, components[variables.GetIndex(0)].allocatorType);
                            return GetInjectArguments(ref injectContext, components[variables.GetIndex(0)].type);
                        }
                    },

                    { "INJECT:FIELD:COUNT", () => components[variables.GetIndex(0)].injectMembers.Length.ToString() },
                    { "INJECT:FIELD:NAME", () => components[variables.GetIndex(0)].injectMembers[variables.GetIndex(1)].fieldName },
                    { "INJECT:FIELD:ARGS", () =>
                        {
                            InjectContextUtils.PrepareContext(ref injectContext, components[variables.GetIndex(0)].allocatorType);
                            return GetInjectArguments(ref injectContext, components[variables.GetIndex(0)].injectMembers[variables.GetIndex(1)].injectParameterDatas);
                        }
                    },

                    { "REPAIR_MEMORY", () => components[variables.GetIndex(0)].isRepairMemory },
                    { "REPAIR_MEMORY:SELF", () => (components[variables.GetIndex(0)]).isRepairMemoryComponent },

                    { "REPAIR_MEMORY:FIELD:COUNT", () => components[variables.GetIndex(0)].repairMemoryMembers.Length.ToString() },
                    { "REPAIR_MEMORY:FIELD:NAME", () => components[variables.GetIndex(0)].repairMemoryMembers[variables.GetIndex(1)].fieldName },

                    { "REPAIR_STATEID", () => components[variables.GetIndex(0)].isRepairStateId },
                    { "REPAIR_STATEID:SELF", () => (components[variables.GetIndex(0)]).isRepairStateIdComponent },

                    { "REPAIR_STATEID:FIELD:COUNT", () => components[variables.GetIndex(0)].repairStateIdMembers.Length.ToString() },
                    { "REPAIR_STATEID:FIELD:NAME", () => components[variables.GetIndex(0)].repairStateIdMembers[variables.GetIndex(1)].fieldName },

                };
            return variables;
        }

        public static TemplateParser.Variables GetState(GeneratorContext context, string stateGenName, ITypeToUshort components, ITypeToUshort configs)
        {
            var fastAccessComponents = new CustomTypeToIdConverter<ushort, IComponent>(
                components.GetAssociationTable().Values.Where(p => new TypeOptions(p).isCompileFastAccess)
                );

            TemplateParser.Variables variables = null;
            variables = new()
                {
                    { "STATE:GEN_NAME", () => stateGenName },

                    { "COMPONENT:COUNT", () => components.GetAssociationTable().Count.ToString() },
                    { "CONFIG:COUNT", () => configs.GetAssociationTable().Count.ToString() },
                    
                    { "COMPONENT:FULL_NAME", () => ReflectionUtils.GetDotFullName(components.IdToType(variables.GetIndexAsId(0))) },
                    { "COMPONENT:FULL_NAME_AS_TEXT", () => ReflectionUtils.GetUnderLineFullName(components.IdToType(variables.GetIndexAsId(0))) },

                    { "CALLER:FASTACCESS:TYPE_NAME", () => TypeOptionsGeneratorUtils.GetCallerFlags(new TypeOptions(fastAccessComponents.IdToType(variables.GetIndexAsId(0)))) },

                    { "COMPONENT:FASTACCESS", () => new TypeOptions(components.IdToType(variables.GetIndexAsId(0))).isCompileFastAccess },
                    { "COMPONENT:FASTACCESS:COUNT", () => fastAccessComponents.GetAssociationTable().Count.ToString() },
                    { "COMPONENT:FASTACCESS:NAME", () => ReflectionUtils.GetUnderLineName(fastAccessComponents.IdToType(variables.GetIndexAsId(0))) },
                    { "COMPONENT:FASTACCESS:FULL_NAME", () => ReflectionUtils.GetDotFullName(fastAccessComponents.IdToType(variables.GetIndexAsId(0))) },

                    { "EMBENED:ComponentInstallerGenerator.cs", () => new ElementsInstallerGenerator().Compile(context, false).First().text },
                    { "EMBENED:SystemInstallerGenerator.cs", () => new SystemInstallerGenerator().Compile(context, false).First().text },
                };
            return variables;
        }

        public static TemplateParser.Variables GetSystem(ITypeToUshort states, ITypeToUshort systems)
        {
            var autoAttaches = systems.GetAssociationTable()
                .Select(p => p.Value)
                .Where(p => p.GetCustomAttribute<ModuleAutoAttachAttribute>() != null)
                .ToArray();

            TemplateParser.Variables variables = null;
            variables = new()
            {
                { "STATE:COUNT", () => states.Count() },
                { "STATE:GEN_NAME", () => states.IdToType(variables.GetIndexAsId(0)).Name },

                { "SYSTEM:COUNT", () => systems.Count() },
                { "SYSTEM:NAME", () => ReflectionUtils.GetDotFullName(systems.IdToType(variables.GetIndexAsId(0))) },

                { "SYSTEM:AUTO_ATTACH:COUNT", () => autoAttaches.Length },
                { "SYSTEM:AUTO_ATTACH:NAME", () => autoAttaches[variables.GetIndex(0)] },
            };
            return variables;
        }

        public static TemplateParser.Variables GetElements(GeneratorContext context)
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

                 { "CONFIG:COUNT", () =>
                    context.GetConfigs(
                        states.IdToType(variables.GetIndexAsId(0))
                    ).Count().ToString()
                },

                { "CONFIG:FULL_NAME", () => ReflectionUtils.GetDotFullName(
                    context.GetConfigs(
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
        
        private static string GetInjectArguments(ref InjectContext context, Type type)
           => GetInjectArguments(
               ref context,
               ReflectionUtils.ExtractInjectParameterData(type)
               .ToArray()
               );

        private static string GetInjectArguments(ref InjectContext context, InjectParameterData[] injectParameterDatas)
        {
            var result = new StringBuilder();

            for (int i = 0; i < injectParameterDatas.Length; ++i)
            {
                var findName = injectParameterDatas[i].Map(ref context);
                findName ??= injectParameterDatas[i].type.Name;

                result.Append($"injectContainer.{findName}");
                if (i < injectParameterDatas.Length - 1)
                {
                    result.Append(",");
                }
            }
            return result.ToString();
        }
    }
}

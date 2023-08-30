using AnotherECS.Converter;
using AnotherECS.Core;
using System;
using System.IO;
using System.Linq;
using System.Text;

namespace AnotherECS.Generator
{
    public class StateGenerator : IFileGenerator
    {
        public string SaveFilePostfixName => "Data.gen.cs";
        public string TemplateFileName => "state.template.txt";


        private const string STATE_NAME_POSTFIX = 
            "Data";

        private const string COMPONENT_HISTORY_DECLARE =
            ", <#IF HISTORY:BYCHANGE#>historyByChangeArgs<#ELSE#>historyByTickArgs<#END#>";

        private const string COMPONENT_VERISON_DECLARE = 
            ", CGGetTickProvider()";

        private const string COMPONENT_INJECT_DECLARE =
            ", ref _injectContainer, Inject.methods[<#COMPONENT_INJECT_DATA_INDEX#>]";

        
        public ContentGenerator[] Compile(GeneratorContext context, bool isForceOverride)
            => context.GetStatesTypes()
            .Select(state => CompileInternal(state, context))
            .Where(p => p.path != null)
            .ToArray();

        public DeleteContentGenerator GetUnusedFiles(GeneratorContext context)
            => default;

        public string[] GetSaveFileNames(GeneratorContext context)
            => context
                .GetStatesTypes()
                .Select(p => GetPathByState(context.GetStatePath(p), p.Name))
                .ExceptDublicates()
                .ToArray();

        private ContentGenerator CompileInternal(Type stateType, GeneratorContext context)
        {
            var path = GetPathByState(context.GetStatePath(stateType), stateType.Name);
            {
                var componentTypes = context.GetComponents(stateType);
                var filterTypes = context.GetFilters(stateType);

                var componentTableDispose = componentTypes.GetAssociationTable()
                    .Where(p => new TypeOptions(p.Value).isDispose)
                    .Select(p => p.Value)
                    .ToArray();

                var componentTableByTick = componentTypes.GetAssociationTable()
                    .Where(p =>
                        {
                            var option = new TypeOptions(p.Value);
                            return option.isHistoryByTick || option.isMarker;
                        }
                    )
                    .Select(p => p.Value)
                    .ToArray();

                var componentCompileDirectAccessCount = componentTypes.GetAssociationTable()
                    .Count(p => new TypeOptions(p.Value).isCompileDirectAccess);

                var componentInject = componentTypes.GetAssociationTable()
                    .Where(p => new TypeOptions(p.Value).isInject)
                    .Select(p => p.Value)
                    .ToArray();

                var tempCounter = 0;
                var componentInjectByType = componentInject.ToDictionary(k => k, v => tempCounter++);

                
                TemplateParser.Variables variables = null;
                variables = new()
                {
                    { "STATE_NAME", p => stateType.Name },
                    { "STATE_NAME_GEN", p => GetStateNameGen(stateType) },
                    { "STORAGE_TYPE", p => TypeOptionsUtils.GetStorageFlags(new TypeOptions(componentTypes.IdToType(p))) },
                    { "ADAPTER_TYPE", p => TypeOptionsUtils.GetAdapterFlags(new TypeOptions(componentTypes.IdToType(p)), context.GetComponentBindWithFilter()) },
                    { "HISTORY_TYPE", p => TypeOptionsUtils.GetHistoryFlags(new TypeOptions(componentTypes.IdToType(p))) },

                    { "DIRECTACCESS", p => new TypeOptions(componentTypes.IdToType(p)).isCompileDirectAccess.ToString() },
                    { "HISTORY:BYCHANGE", p => new TypeOptions(componentTypes.IdToType(p)).isHistoryByChange.ToString() },

                    { "SHARED", p => new TypeOptions(componentTypes.IdToType(p)).isShared.ToString() },
                    { "COMPONENT_NAME", p => componentTypes.IdToType(p).Name },
                    { "COMPONENT_FULL_NAME", p => componentTypes.IdToType(p).FullName },
                    { "COMPONENT_NAME_LOW", p => componentTypes.IdToType(p).Name.ToLower() },
                    { "COMPONENT_COUNT", p => componentTypes.GetAssociationTable().Count.ToString() },
                    { "COMPONENT_COUNT+1", p => (componentTypes.GetAssociationTable().Count + 1).ToString() },

                    { "COMPONENT_DIRECTACCESS_COUNT", p => componentCompileDirectAccessCount.ToString() },

                    { "COMPONENT_DISPOSE_NAME_LOW", p => componentTableDispose[p].Name.ToLower() },
                    { "COMPONENT_DISPOSE_COUNT", p => componentTableDispose.Length.ToString() },
                    { "COMPONENT_DISPOSE_COUNT+1", p => (componentTableDispose.Length + 1).ToString() },

                    { "COMPONENT_HISTORY_DECLARE", p => new TypeOptions(componentTypes.IdToType(p)).isHistory ? COMPONENT_HISTORY_DECLARE : string.Empty },
                    { "COMPONENT_VERISON_DECLARE", p => new TypeOptions(componentTypes.IdToType(p)).isVersion ? COMPONENT_VERISON_DECLARE : string.Empty },
                    { "COMPONENT_INJECT_DECLARE", p => new TypeOptions(componentTypes.IdToType(p)).isInject ? COMPONENT_INJECT_DECLARE : string.Empty },

                    { "FILTER_NAME", p => ReflectionUtils.GetGeneratorFullName(filterTypes.IdToType(p)) },
                    { "FILTER_NAME_FUCTION", p => ReflectionUtils.GetGeneratorFullName(filterTypes.IdToType(p)).Replace('.', '_') },
                    
                    { "FILTER_ISAUTOCLEAR", p => IsAutoClear<IInclude>(filterTypes.IdToType(p)).ToString().ToLower() },
                    { "FILTER_INCLUDES", p => GetMask<IInclude>(filterTypes.IdToType(p), componentTypes) },
                    { "FILTER_EXCLUDES", p => GetMask<IExclude>(filterTypes.IdToType(p), componentTypes) },
                    { "FILTER_RULE", p => GetRule(filterTypes.IdToType(p)) },
                    { "FILTER_COUNT", p => filterTypes.GetAssociationTable().Count.ToString() },
                    { "FILTER_COUNT+1", p => (filterTypes.GetAssociationTable().Count + 1).ToString() },

                    { "COMPONENT_BY_TICK_NAME_LOW", p => componentTableByTick[p].Name.ToLower() },
                    { "COMPONENT_BY_TICK_COUNT", p => componentTableByTick.Length.ToString() },
                    { "COMPONENT_BY_TICK_COUNT+1", p => (componentTableByTick.Length + 1).ToString() },

                    { "EMBEDCODE SystemGenerator", p => new SystemGenerator(stateType).Compile(context, true).First().text },

                    { "DECLARE:componentCapacity", p => new TypeOptions(componentTypes.IdToType(p)).isHistoryByTick ? "componentCapacity, " : string.Empty },

                    { "STORAGE_CAPACITY", p => { var option = new TypeOptions(componentTypes.IdToType(p)); return option.isOverrideCapacity ? option.capacity.ToString() : "componentCapacity"; } },

                    { "COMPONENT_INJECT_COUNT", p => componentInject.Length.ToString() },
                    { "INJECT_SELF", p => new TypeOptions(componentInject[p]).isInjectComponent.ToString() },

                    { "INJECT_SELF_ARGS", p => GetInjectArguments(componentInject[p]) },
                    { "INJECT_ARGS", p => GetInjectArguments(new TypeOptions(componentInject[variables.GetIndex(0)]).injectMembers[variables.GetIndex(1)].argumentTypes) },

                    { "INJECT_FIELD_COUNT", p => new TypeOptions(componentInject[p]).injectMembers.Length.ToString() },
                    { "INJECT_FIELD_NAME", p => new TypeOptions(componentInject[variables.GetIndex(0)]).injectMembers[variables.GetIndex(1)].fieldName },
                    { "COMPONENT_INJECT_NAME", p => componentInject[p].Name },
                    { "COMPONENT_INJECT_FULL_NAME", p => componentInject[p].FullName },

                    { "COMPONENT_INJECT_DATA_INDEX", p => componentInjectByType[componentTypes.IdToType(p)].ToString() },
                };

                return new ContentGenerator(path, TemplateParser.Transform(context.GetTemplate(TemplateFileName), variables));
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

            for(int i = 0; i < types.Length; ++i)
            {
                result.Append($"injectContainer.{types[i]}");
                if (i < types.Length - 1)
                {
                    result.Append(",");
                }
            }
            return result.ToString();    
        }

        private static string GetStateNameGen(Type stateType)
            => stateType.Name + STATE_NAME_POSTFIX;

        private static bool IsAutoClear<T>(Type type)
        {
            var types = ReflectionUtils.ExtractGenericFromInterface<T>(type);
            return types.Length != 0 && types.Any(ComponentUtils.IsMarker);
        }

        private static string GetMask<T>(Type type, ITypeToUshort converter)
        {
            var types = ReflectionUtils.ExtractGenericFromInterface<T>(type);
            if (types.Length == 0)
            {
                return "null";
            }

            var body = types
                .Select(p => converter.TypeToId(p))
                .OrderBy(p => p)
                .Select(p => p.ToString())
                .Aggregate((s, p) => s + ", " + p);

            return "new ushort[] { " + body + " }";
        }

        private static string GetRule(Type type)
        {
            var includes = ReflectionUtils.ExtractGenericFromInterface<IInclude>(type);
            var excludes = ReflectionUtils.ExtractGenericFromInterface<IExclude>(type);

            var rule0 = GetRuleString(includes, true);
            var rule1 = GetRuleString(excludes, false);

            if (rule0 != null && rule1 != null)
            {
                return rule0 + " && " + rule1;
            }
            else if (rule0 != null)
            {
                return rule0;
            }
            else if (rule1 != null)
            {
                return rule1;
            }

            throw new Exceptions.FilterHasNoConditionException(type.Name);
        }

        private static string GetRuleString(Type[] filterTypes, bool isInclude)
        {
            if (filterTypes.Length == 0)
                return null;

            var result = new StringBuilder();
            for (int i = 0; i < filterTypes.Length; ++i)
            {
                if (!isInclude)
                {
                    result.Append("!");
                }

                if (ComponentUtils.IsCompileDirectAccess(filterTypes[i]))
                {
                    result.Append($"s.{filterTypes[i].Name}.IsHas(id)");
                }
                else
                {
                    result.Append($"s.IsHas<{filterTypes[i].Name}>(id)");
                }
                if (i != filterTypes.Length - 1)
                {
                    result.Append(" && ");
                }
            }
            return result.ToString();
        }

        private string GetPathByState(string path, string stateName)
            => Path.Combine(path, stateName + SaveFilePostfixName);
    }
}

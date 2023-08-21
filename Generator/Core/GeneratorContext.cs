using AnotherECS.Converter;
using AnotherECS.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AnotherECS.Generator
{
    public class GeneratorContext
    {
        private readonly RuntimeStateConverter _stateConverter;
        private readonly RuntimeOrderSystem _systemConverter;
        private readonly Type[] _ignoreTypes;
        private readonly IEnvironmentProvider _environmentProvider;
        private ComponentFilterData? _componentBindWithFilters = null;

        public GeneratorContext(IEnvironmentProvider environmentProvider)
            : this(null, environmentProvider)
        { }

        public GeneratorContext(Type[] ignoreTypes, IEnvironmentProvider environmentProvider)
        {
            _ignoreTypes = ignoreTypes ?? Array.Empty<Type>();
            _systemConverter = new RuntimeOrderSystem(_ignoreTypes);
            _stateConverter = new RuntimeStateConverter(_ignoreTypes);
            _environmentProvider = environmentProvider;
        }
        
        public ITypeToUshort GetStates()
            => _stateConverter;

        public ITypeToUshort GetSystems()
            => _systemConverter;

        public ITypeToUshort GetFilters(Type stateType)
        {
            var type = typeof(RuntimeFilterToIntConverter<>);
            var genericType = type.MakeGenericType(stateType);
            return (ITypeToUshort)Activator.CreateInstance(genericType, new[] { _ignoreTypes });
        }

        public ITypeToUshort GetComponents(Type stateType)
        {
            var type = typeof(RuntimeComponentToIntConverter<>);
            var genericType = type.MakeGenericType(stateType);
            return (ITypeToUshort)Activator.CreateInstance(genericType, new[] { _ignoreTypes });
        }

        public string GetStatePath(Type stateType)
            => _environmentProvider.GetFilePathToType(stateType);

        public string GetTemplate(string fileName)
            => _environmentProvider.GetTemplate(fileName);

        public string FindRootGenCommonDirectory()
            => _environmentProvider.FindRootGenCommonDirectory();

        public Type[] GetStatesTypes()
            => _stateConverter.GetAssociationTable().Values
                .ToArray();


        public Type[] GetFilterTypes()
            => GetStatesTypes()
                .Select(p => GetFilters(p).GetAssociationTable().Values)
                .SelectMany(p => p)
                .ToArray();

        public Type[] GetFilterTypesExceptDublicates()
            => GetFilterTypes()
                .ExceptDublicates()
                .ToArray();

        public Type[] GetComponentTypes()
            => GetStatesTypes()
                .Select(p => GetComponents(p).GetAssociationTable().Values)
                .SelectMany(p => p)
                .ToArray();

        public Type[] GetComponentTypesExceptDublicates()
            => GetComponentTypes()
                .ExceptDublicates()
                .ToArray();

        public Type[] GetAllTypes()
            => GetStatesTypes()
                .Union(GetFilterTypes())
                .Union(GetComponentTypes())
                .ToArray();

        public Type[] GetAllTypesExceptDublicates()
            => GetAllTypes()
                .ExceptDublicates()
                .ToArray();

        public ComponentFilterData GetComponentBindWithFilter()
        {
            if (!_componentBindWithFilters.HasValue)
            {
                var filters = GetFilterTypesExceptDublicates();
                var includes = new HashSet<Type>();
                var excludes = new HashSet<Type>();

                foreach (var filter in filters)
                {
                    var includeTypes = ReflectionUtils.ExtractGenericFromInterface<IInclude>(filter);
                    foreach (var type in includeTypes)
                    {
                        if (!includes.Contains(type))
                        {
                            includes.Add(type);
                        }
                    }
                    var excludeTtypes = ReflectionUtils.ExtractGenericFromInterface<IExclude>(filter);
                    foreach (var type in excludeTtypes)
                    {
                        if (!excludes.Contains(type))
                        {
                            excludes.Add(type);
                        }
                    }
                }
                _componentBindWithFilters = new(includes, excludes);
            }

            return _componentBindWithFilters.Value;
        }

        public struct ComponentFilterData
        {
            public HashSet<Type> includes;
            public HashSet<Type> excludes;

            public ComponentFilterData(HashSet<Type> includes, HashSet<Type> excludes)
            {
                this.includes = includes;
                this.excludes = excludes;
            }

            public void Deconstruct(out HashSet<Type> includes, out HashSet<Type> excludes)
            {
                includes = this.includes;
                excludes = this.excludes;
            }
        }
    }
}

﻿using System;
using System.Linq;
using AnotherECS.Converter;
using AnotherECS.Core;

namespace AnotherECS.Generator
{
    public class GeneratorContext
    {
        private readonly RuntimeStateConverter _stateConverter;
        private readonly RuntimeOrderSystem _systemConverter;
        private readonly Type[] _ignoreTypes;
        private readonly IEnvironmentProvider _environmentProvider;

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

        public Type[] GetComponents()
             => GetStateTypes()
                 .Select(p => GetComponents(p).GetAssociationTable().Values)
                 .SelectMany(p => p)
                 .ExceptDublicates()
                 .ToArray();

        public Type[] GetAllTypes()
            => GetStateTypes()
                .Union(GetComponents())
                .ExceptDublicates()
                .ToArray();

        public Type[] GetStateTypes()
            => GetStates().GetAssociationTable().Values
                .ToArray();
    }
}

using System;
using System.Linq;
using AnotherECS.Converter;
using AnotherECS.Core;

namespace AnotherECS.Generator
{
    public class GeneratorContext
    {
        private readonly RuntimeStateConverter _stateConverter;
        private readonly RuntimeSystemConverter _systemConverter;
        private readonly Type[] _ignoreTypes;
        private readonly IEnvironmentProvider _environmentProvider;

        public GeneratorContext(IEnvironmentProvider environmentProvider)
            : this(null, environmentProvider)
        { }

        public GeneratorContext(Type[] ignoreTypes, IEnvironmentProvider environmentProvider)
        {
            _ignoreTypes = ignoreTypes ?? Array.Empty<Type>();
            _systemConverter = new RuntimeSystemConverter(_ignoreTypes);
            _stateConverter = new RuntimeStateConverter(_ignoreTypes);
            _environmentProvider = environmentProvider;
        }
        
        public ITypeToUshort GetStates()
            => _stateConverter;

        public ITypeToUshort GetSystems()
            => _systemConverter;

        public ITypeToUshort GetComponents(Type stateType)
        {
            var type = typeof(RuntimeComponentConverter<>);
            var genericType = type.MakeGenericType(stateType);
            return (ITypeToUshort)Activator.CreateInstance(genericType, new[] { _ignoreTypes });
        }

        public Type[] GetComponents()
            => new IgnoresTypeToIdConverter<ushort, IComponent>(_ignoreTypes)
                .GetAssociationTable().Values.ToArray();

        public string GetStatePath(string stateName)
            => _environmentProvider.GetFilePathByStateName(stateName);

        public string GetTemplate(string fileName)
            => _environmentProvider.GetTemplate(fileName);

        public string FindRootGenDirectory()
            => _environmentProvider.FindRootGenDirectory();

        public string FindRootGenCommonDirectory()
            => _environmentProvider.FindRootGenCommonDirectory();
      
        public Type[] GetAllTypes()
            => GetStateTypes()
                .Union(GetComponents())
                .Union(GetSystems().GetAssociationTable().Values)
                .ExceptDublicates()
                .ToArray();

        public Type[] GetStateTypes()
            => GetStates().GetAssociationTable().Values
                .ToArray();
    }
}

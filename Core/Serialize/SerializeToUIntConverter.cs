using System;
using System.Collections.Generic;
using System.Linq;
using AnotherECS.Converter;
using AnotherECS.Core;

namespace AnotherECS.Serializer
{
    public class SerializeToUIntConverter : SerializeToUInt
    {
        private readonly (uint id, Type type)[] _iSerializeres;

        private readonly Dictionary<uint, Type> _direct;
        private readonly Dictionary<Type, uint> _reverse;

        public SerializeToUIntConverter(uint startId)
        {
            var iSerializers = new IgnoresTypeToIdConverter<uint, IElementSerializer>().GetAssociationTable();
            var iSerializes = new IgnoresTypeToIdConverter<uint, ISerialize>().GetAssociationTable();
            var iComponents = new IgnoresTypeToIdConverter<uint, IComponent>().GetAssociationTable();
            var iEvents = new IgnoresTypeToIdConverter<uint, IEvent>().GetAssociationTable();
            
            var serializeAttributes = TypeUtils.GetAllowHasAttributeFromTypesAcrossAll<SerializeAttribute>();

            uint id = startId;
            _reverse = new();
            _iSerializeres = new (uint id, Type type)[iSerializers.Count];

            foreach (var item in iSerializers.Values
                .OrderBy(p => p.Name))
            {
                var type = ExtractElementTypeFromISerializerType(item);

                if (!_reverse.ContainsKey(type))
                {
                    _reverse.Add(type, id);
                    _iSerializeres[id - startId] = (id, item);
                    ++id;
                }
            }

            foreach (var item in iSerializes.Values
                .Union(iComponents.Values)
                .Union(iEvents.Values)
                .Union(serializeAttributes)
                .OrderBy(p => p.Name)
                )
            {
                if (!_reverse.ContainsKey(item))
                {
                    _reverse.Add(item, id++);
                }
            }

            _direct = _reverse.ToDictionary(p => p.Value, p => p.Key);
        }

        private Type ExtractElementTypeFromISerializerType(Type type)
            => (Activator.CreateInstance(type) as IElementSerializer).Type;

        public (uint id, Type iSerializereTypes)[] GetISerializeres()
            => _iSerializeres;

        public Type IdToType(uint id)
            => _direct[id];

        public uint TypeToId(Type type)
            => _reverse[type];

        public Dictionary<uint, Type> GetAssociationTable()
            => _direct;
    }
}

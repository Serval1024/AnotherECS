using System;
using System.Collections.Generic;
using System.Linq;

namespace AnotherECS.Serializer
{
    public class SerializeToUIntConverter : ITypeToUInt
    {
        private readonly Dictionary<uint, Type> _direct;
        private readonly Dictionary<Type, uint> _reverse;

        public SerializeToUIntConverter(uint startId, IEnumerable<Type> types)
        {
            uint id = startId;
            _reverse = new();

            foreach (var item in types
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

        public Type IdToType(uint id)
            => _direct[id];

        public uint TypeToId(Type type)
            => _reverse[type];

        public Dictionary<uint, Type> GetAssociationTable()
            => _direct;
    }
}

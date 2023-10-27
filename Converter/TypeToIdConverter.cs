using System;
using System.Collections.Generic;
using System.Linq;

namespace AnotherECS.Converter
{
    public abstract class TypeToIdConverter<UId> : ITypeToId<UId>
        where UId : unmanaged
    {
        private Dictionary<UId, Type> _direct;
        private Dictionary<Type, UId> _reverse;

        public TypeToIdConverter()
        {
            OnInit();
        }

        public Dictionary<UId, Type> GetAssociationTable()
            => _direct.ToDictionary(k => k.Key, v => v.Value);

        public Type IdToType(UId id)
            => _direct[id];

        public UId TypeToId(Type type)
            => _reverse[type];

        protected void Init()
        {
            _direct = GetSortAssociationTableInternal();
            _reverse = _direct?.ToDictionary(p => p.Value, p => p.Key);
        }

        protected virtual void OnInit()
            => Init();

        protected abstract IEnumerable<Type> GetSortTypes();

        protected Dictionary<UId, Type> GetSortAssociationTableInternal()
        {
            var types = GetSortTypes().ToArray();
            var result = new Dictionary<UId, Type>();
            dynamic idCounter = 0;
            for (int i = 0; i < types.Length; ++i)
            {
                ++idCounter;
                result.Add((UId)idCounter, types[i]);
            }

            return result;
        }
    }
}


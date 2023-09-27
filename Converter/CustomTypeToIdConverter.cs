using System;
using System.Collections.Generic;

namespace AnotherECS.Converter
{
    public class CustomTypeToIdConverter<UId, TType> : TypeToIdConverter<UId, TType>
        where UId : unmanaged
        where TType : class
    {
        private readonly IEnumerable<Type> _types;

        public CustomTypeToIdConverter(IEnumerable<Type> types)
        {
            _types = types;
            Init();
        }

        protected override void OnInit() { }

        protected override IEnumerable<Type> GetSortTypes()
           => _types;
    }
}

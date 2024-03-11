using AnotherECS.Converter;
using System;
using System.Collections.Generic;

namespace AnotherECS.Serializer
{
    public struct ReflectionElementSerializersProvider
    {
        private IEnumerable<IElementSerializer> _resultCache;

        public IEnumerable<IElementSerializer> Gets()
        {
            if (_resultCache == null)
            {
                var iElementSerializers = new IgnoresTypeToIdConverter<uint, IElementSerializer>().GetAssociationTable();

                var gate = new HashSet<Type>();
                var result = new List<IElementSerializer>();

                foreach (var item in iElementSerializers.Values)
                {
                    var inst = Activator.CreateInstance(item) as IElementSerializer;

                    if (!gate.Contains(item))
                    {
                        gate.Add(item);
                        result.Add(inst);
                    }
                }
                _resultCache = result;
            }

            return _resultCache;
        }
    }
}
using System;

namespace AnotherECS.Serializer
{
    public struct CompoundMeta
    {
        public void Pack(ref WriterContextSerializer writer, object value)
        {
            foreach (var member in SerializerUtils.GetMembers(value.GetType()))
            {
                writer.Pack(SerializerUtils.GetValue(member, value));
            }
        }

        public object Unpack(ref ReaderContextSerializer reader, Type type)
        {
            var instance = Activator.CreateInstance(type);
            foreach (var member in SerializerUtils.GetMembers(type))
            {
                SerializerUtils.SetValue(member, instance, reader.Unpack());
            }

            return instance;
        }
    }
}

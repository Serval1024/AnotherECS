namespace AnotherECS.Serializer
{
    public struct DependencySerializer
    {
        public uint id;
        public object value;

        public DependencySerializer(object value)
            : this(0, value) { }

        public DependencySerializer(uint id, object value)
        {
            this.id = id;
            this.value = value;
        }
    }
}

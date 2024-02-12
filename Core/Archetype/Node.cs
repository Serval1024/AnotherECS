namespace AnotherECS.Core
{
    internal unsafe struct Node
    {
        public uint parent;

        public uint archetypeId;
        public uint itemId;

        public uint collectionId;

        public uint hash;

        public uint childrenCapacity;
        public uint childrenCount;
        public uint childrenId;
    }
}
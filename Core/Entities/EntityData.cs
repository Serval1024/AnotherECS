using System.Runtime.InteropServices;
using AnotherECS.Converter;

namespace AnotherECS.Core
{
    [IgnoreCompile]
    internal unsafe struct EntityData : IComponent
    {
        public uint archetypeId;
        public ushort generation;
    }
}

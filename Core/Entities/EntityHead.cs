using System.Runtime.InteropServices;
using AnotherECS.Converter;

namespace AnotherECS.Core
{
    [IgnoreCompile]
    [StructLayout(LayoutKind.Sequential, Size = 32)]
    internal unsafe struct EntityHead : IComponent   //TODO SER SIZE CHECK, and for unmanagedlayout. UnitTest here
    {
        public const ushort ComponentMax = 12;

        public uint next;
        public ushort generation;
        public ushort count;
        public fixed ushort components[ComponentMax];
    }

    [IgnoreCompile]
    [StructLayout(LayoutKind.Sequential, Size = 32)]
    internal unsafe struct EntityTail : IComponent
    {
        public const ushort ComponentMax = 13;

        public uint next;
        private readonly ushort GENERATION_ZERO_SPACE;
        public fixed ushort components[ComponentMax];
    }
}

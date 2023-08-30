using System.Runtime.InteropServices;

namespace AnotherECS.Core
{
    [StructLayout(LayoutKind.Sequential, Size = 32)]
    internal unsafe struct EntityHead
    {
        public const ushort ComponentMax = 12;

        public uint next;
        public ushort generation;
        public ushort count;
        public fixed ushort components[ComponentMax];
    }

    [StructLayout(LayoutKind.Sequential, Size = 32)]
    internal unsafe struct EntityTail
    {
        public const ushort ComponentMax = 13;

        public uint next;
        private ushort GENERATION_ZERO_SPACE;
        public fixed ushort components[ComponentMax];
    }
}

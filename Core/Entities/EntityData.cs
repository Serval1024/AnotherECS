using System.Runtime.InteropServices;

namespace AnotherECS.Core
{
    [StructLayout(LayoutKind.Explicit, Size = 6)]
    internal unsafe struct EntityData
    {
        [FieldOffset(0)] public uint archetypeId;
        [FieldOffset(4)] public ushort generation;
    }
}

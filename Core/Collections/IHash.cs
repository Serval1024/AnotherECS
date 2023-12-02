using System.Runtime.CompilerServices;

namespace AnotherECS.Core.Collection
{
    public interface IHash<TKey, THash>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        THash GetHash(ref TKey key);
    }

    public struct U2U8HashProvider : IHash<ushort, ulong>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ulong GetHash(ref ushort key)
            => key;
    }

    public struct U2U4HashProvider : IHash<ushort, uint>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint GetHash(ref ushort key)
            => key;
    }

    public struct U4U4HashProvider : IHash<uint, uint>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint GetHash(ref uint key)
            => key;
    }

    public struct U4U8HashProvider : IHash<uint, ulong>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ulong GetHash(ref uint key)
            => key;
    }

    public struct U8U8HashProvider : IHash<ulong, ulong>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ulong GetHash(ref ulong key)
            => key;
    }
}
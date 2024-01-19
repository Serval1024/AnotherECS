using System.Runtime.CompilerServices;

namespace AnotherECS.Core.Collection
{
    public interface IHashProvider<TKey, THash>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        THash GetHash(ref TKey key);
    }

    public struct U2U4HashProvider : IHashProvider<ushort, uint>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint GetHash(ref ushort key)
            => key;
    }

    public struct U4U4HashProvider : IHashProvider<uint, uint>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint GetHash(ref uint key)
            => key;
    }

    public struct U8U4HashProvider : IHashProvider<ulong, uint>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint GetHash(ref ulong key)
            => (uint)((key >> 32) ^ (key));
    }
}
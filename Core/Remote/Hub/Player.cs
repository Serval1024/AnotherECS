using System;
using System.Runtime.CompilerServices;

namespace AnotherECS.Core.Remote
{
    public readonly struct Player : IEquatable<Player>
    {
        public long Id { get; }
        public ClientRole Role { get; }

        public Player(long id, ClientRole role)
        {
            Id = id;
            Role = role;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(Player lhs, Player rhs)
            => lhs.Equals(rhs);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(Player lhs, Player rhs)
            => !lhs.Equals(rhs);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(Player other)
            => Id == other.Id && Role == other.Role;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object obj)
            => obj is Player player && Equals(player);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode()
            => HashCode.Combine(Id, Role);
    }
}

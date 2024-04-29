using System;
using System.Runtime.CompilerServices;

namespace AnotherECS.Core.Remote
{
    public readonly struct Player : IEquatable<Player>
    {
        public long Id { get; }
        public bool IsLocal { get; }
        public ClientRole Role { get; }
        public long PerformanceTiming { get; }

        public bool IsValid => Id != 0 && Role != ClientRole.None;

        public Player(long id, bool isLocal, ClientRole role, long performanceTiming)
        {
            Id = id;
            IsLocal = isLocal;
            Role = role;
            PerformanceTiming = performanceTiming;
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

using AnotherECS.Serializer;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace AnotherECS.Core
{
    public struct WorldData : IDisposable, ISerialize
    {
        internal uint CurrentTick => State == null ? 0 : State.Tick;
        internal bool IsEmpty => State == null;


        internal bool IsOneGateCallCreate;
        internal bool IsOneGateCallDestroy;
        internal bool IsOneGateAutoAttach;

        internal IGroupSystemInternal Systems;
        internal State State;

        public WorldData(IEnumerable<ISystem> systems, State state)
        {
            Systems = new SystemGroup(
                systems ?? throw new ArgumentNullException(nameof(systems))
            );

            State = state;

            IsOneGateCallCreate = true;
            IsOneGateCallDestroy = true;
            IsOneGateAutoAttach = true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(WorldData p0, WorldData p1)
            => p0.IsOneGateCallCreate == p1.IsOneGateCallCreate &&
               p0.IsOneGateCallDestroy == p1.IsOneGateCallDestroy &&
               p0.IsOneGateAutoAttach == p1.IsOneGateAutoAttach &&
               p0.Systems == p1.Systems &&
               p0.State == p1.State;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(WorldData p0, WorldData p1)
            => !(p0 == p1);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(WorldData other)
            => this == other;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object obj)
            => (obj is WorldData entity) && Equals(entity);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode()
        {
            HashCode hash = default;
            hash.Add(IsOneGateCallCreate);
            hash.Add(IsOneGateCallDestroy);
            hash.Add(IsOneGateAutoAttach);
            hash.Add(Systems);
            hash.Add(State);
            return hash.ToHashCode();
        }

        public void Dispose()
        {
            Systems?.Dispose();
            State?.Dispose();
        }

        public void Pack(ref WriterContextSerializer writer)
        {
            writer.Write(IsOneGateCallCreate);
            writer.Write(IsOneGateCallDestroy);
            writer.Pack(Systems);
            writer.Pack(State);
        }

        public void Unpack(ref ReaderContextSerializer reader)
        {
            IsOneGateCallCreate = reader.ReadBoolean();
            IsOneGateCallDestroy = reader.ReadBoolean();
            reader.Unpack(Systems);
            reader.Unpack(State);
            IsOneGateAutoAttach = false;
        }        
    }
}

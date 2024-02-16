using System;
using System.Runtime.CompilerServices;

namespace AnotherECS.Core
{
#if ENABLE_IL2CPP
    [Unity.IL2CPP.CompilerServices.Il2CppSetOption(Option.NullChecks, false)]
    [Unity.IL2CPP.CompilerServices.Il2CppSetOption(Option.ArrayBoundsChecks, false)]
#endif
    public static class StateGlobalRegister
    {
        private static readonly MRecycle _recycle = new(16);
        private static State[] _data = new State[16];

#if UNITY_EDITOR
        [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ReloadDomainOptimizationHack()
        {
            Array.Clear(_data, 0, _data.Length);
        }
#endif

        public static ushort Register(State state)
        {
            lock (_data)
            {
                var id = _recycle.Allocate();
                if (id >= _data.Length)
                {
                    var newArray = new State[id + 1];
                    Array.Copy(_data, newArray, _data.Length);
                    _data = newArray;
                }

                _data[id] = state;
                return (ushort)id;
            }
        }

        public static void Unregister(ushort id)
        {
            lock (_data)
            {
                _recycle.Deallocate(id);
                _data[id] = default;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static State Get(ushort id)
            => _data[id];
    }
}

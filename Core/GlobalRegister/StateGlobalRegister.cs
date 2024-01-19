using System.Collections.Generic;

namespace AnotherECS.Core
{
#if ENABLE_IL2CPP
    [Unity.IL2CPP.CompilerServices.Il2CppSetOption(Option.NullChecks, false)]
    [Unity.IL2CPP.CompilerServices.Il2CppSetOption(Option.ArrayBoundsChecks, false)]
#endif
    public static class StateGlobalRegister
    {
        private static readonly MRecycle _recycle = new(16);
        private static readonly List<State> _data = new();

#if UNITY_EDITOR
        [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ReloadDomainOptimizationHack()
        {
            _data.Clear();
        }
#endif

        public static ushort Register(State state)
        {
            lock (_data)
            {
                var id = _recycle.Allocate();
                while (id >= _data.Count)
                {
                    _data.Add(default);
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

        public static State Get(ushort id)
            => _data[id];
    }
}

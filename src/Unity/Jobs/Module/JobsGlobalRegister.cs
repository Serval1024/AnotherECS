using AnotherECS.Core;
using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace AnotherECS.Unity.Jobs
{
    public static class JobsGlobalRegister
    {
        private static NativeArrayProvider[] _data = new NativeArrayProvider[16];

#if UNITY_EDITOR
        [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ReloadDomainOptimizationHack()
        {
            Array.Clear(_data, 0, _data.Length);
        }
#endif

        public static void Register(State state)
        {
            var id = state.GetStateId();
            lock (_data)
            {
                if (id >= _data.Length)
                {
                    var newArray = new NativeArrayProvider[Math.Max(id + 1, _data.Length << 1)];
                    Array.Copy(_data, newArray, _data.Length);
                    Thread.MemoryBarrier();
                    _data = newArray;
                }

                _data[id] = new NativeArrayProvider(state);
            }
        }

        public static void Unregister(State state)
        {
            var id = state.GetStateId();
            NativeArrayProvider data;
            lock (_data)
            {
                data = _data[id];
                _data[id] = default;
            }
            data.Dispose();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeArrayProvider Get(State state)
            => _data[state.GetStateId()];
    }
}



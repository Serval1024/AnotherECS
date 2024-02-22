using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace AnotherECS.Core
{
#if ENABLE_IL2CPP
    [Unity.IL2CPP.CompilerServices.Il2CppSetOption(Option.NullChecks, false)]
    [Unity.IL2CPP.CompilerServices.Il2CppSetOption(Option.ArrayBoundsChecks, false)]
#endif
    public class IdRegister<T>
    {
        private MRecycle _recycle = new(16);
        private T[] _data = new T[16];

        public ushort Register(T data)
        {
            lock (_data)
            {
                var id = _recycle.Allocate();
                if (id >= _data.Length)
                {
                    var newArray = new T[Math.Max(id + 1, _data.Length << 1)];
                    Array.Copy(_data, newArray, _data.Length);
                    Thread.MemoryBarrier();
                    _data = newArray;
                }

                _data[id] = data;
                return (ushort)id;
            }
        }

        public void Unregister(ushort id)
        {
            lock (_data)
            {
                _recycle.Deallocate(id);
                _data[id] = default;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Get(ushort id)
            => _data[id];

        public void Clear()
        {
            Array.Clear(_data, 0, _data.Length);
        }
    }
}
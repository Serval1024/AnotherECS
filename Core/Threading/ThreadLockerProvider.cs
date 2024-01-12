using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace AnotherECS.Core.Threading
{
    public struct ThreadLockerProvider
    {
        private List<object> _lockers;
        private MRecycle _recycle;

        public ThreadLockerProvider(int capacity)
        {
            _lockers = new List<object>(capacity);
            _recycle = new MRecycle(16);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int AllocateId()
        {
            lock (_lockers)
            {
                int id = _recycle.Allocate();
                while (id >= _lockers.Count)
                {
                    _lockers.Add(new object());
                }
                return id;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DeallocateId(int id)
        {
            lock (_lockers)
            {
                _recycle.Deallocate(id);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public object GetLocker(int id)
#if ANOTHERECS_RELEASE
            => _lockers[id];
#else
        {
            if (id <= 0)
            {
                throw new System.ArgumentException(nameof(id));
            }
            return _lockers[id];
        }
#endif
    }
}

using System.Runtime.CompilerServices;

namespace AnotherECS.Core
{
    public struct NeedRefreshByTick
    {
        private bool _isActive;
        private uint _activeTick;
        private uint _recordLength;

        public NeedRefreshByTick(uint recordLength)
        {
            _isActive = false;
            _activeTick = 0;
            _recordLength = recordLength;
        }

        public bool IsActive
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _isActive;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(uint tick)
        {
            _isActive = true;
            _activeTick = tick;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryDrop(uint tick)
        {
            if (_isActive)
            {
                if (tick > _recordLength + _activeTick)
                {
                    _isActive = false;
                    return false;
                }
            }
            return true;
        }
    }
}

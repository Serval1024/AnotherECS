using System.Runtime.CompilerServices;

namespace AnotherECS.Core.Caller
{
    public struct ADExternalContext
    {
        internal State _state;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T GetConfig<T>()
            where T : IConfig
            => _state.GetConfig<T>();
    }
}

using System.Runtime.CompilerServices;

namespace AnotherECS.Core.Caller
{
    internal unsafe struct DefaultFeature<TDense> : IData, IDefaultSetter<TDense>
        where TDense : struct, IDefault
    {
        public State state;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Allocate(State state, GlobalDependencies* dependencies)
        {
            this.state = state;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose() { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetupDefault(ref TDense component)
        {
            component.Setup(state);
        }
    }
}

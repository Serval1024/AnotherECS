using System.Runtime.CompilerServices;

namespace AnotherECS.Core.Caller
{
    internal struct DefaultFeature<TDense> : IDefaultSetter<TDense>
        where TDense : struct, IDefault
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetupDefault(ref TDense component)
        {
            component.Setup();
        }
    }
}

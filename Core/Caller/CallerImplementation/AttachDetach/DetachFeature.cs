using System.Runtime.CompilerServices;
using AnotherECS.Core.Actions;
using AnotherECS.Core.Collection;

namespace AnotherECS.Core.Caller
{
    internal unsafe struct DetachFeature<TAllocator, TSparse, TDense, TDenseIndex> : IDetach<TAllocator, TSparse, TDense, TDenseIndex>, IBoolConst
        where TAllocator : unmanaged, IAllocator
        where TSparse : unmanaged
        where TDense : unmanaged, IDetach
        where TDenseIndex : unmanaged
    {
        public bool Is { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => true; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Detach<JSparseBoolConst>(UnmanagedLayout<TAllocator, TSparse, TDense, TDenseIndex>* layout, State state, uint startIndex, uint count)
            where JSparseBoolConst : struct, IBoolConst
        {
            if (default(JSparseBoolConst).Is)
            {
                var layoutBool = (UnmanagedLayout<TAllocator, bool, TDense, TDenseIndex>*)layout;
                DetachLayoutActions.Detach_bool(ref *layoutBool, state, startIndex);
            }
            else
            {
                var layoutUshort = (UnmanagedLayout<TAllocator, ushort, TDense, TDenseIndex>*)layout;
                DetachLayoutActions.Detach_ushort(ref *layoutUshort, state, count);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Detach<JSparseBoolConst>(UnmanagedLayout<TAllocator, TSparse, TDense, TDenseIndex>* layout, State state, ref NArray<BAllocator, Op> ops)
            where JSparseBoolConst : struct, IBoolConst
        {
            if (default(JSparseBoolConst).Is)
            {
                var layoutBool = (UnmanagedLayout<TAllocator, bool, TDense, TDenseIndex>*)layout;
                DetachLayoutActions.Detach_bool(ref *layoutBool, state, ref ops);
            }
            else
            {
                var layoutUshort = (UnmanagedLayout<TAllocator, ushort, TDense, TDenseIndex>*)layout;
                DetachLayoutActions.Detach_ushort(ref *layoutUshort, state, ref ops);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Detach(State state, ref TDense component)
        {
            component.OnDetach(state);
        }
    }
}

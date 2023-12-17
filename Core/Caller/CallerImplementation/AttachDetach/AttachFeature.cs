using System.Runtime.CompilerServices;
using AnotherECS.Core.Actions;
using AnotherECS.Core.Collection;

namespace AnotherECS.Core.Caller
{
    internal unsafe struct AttachFeature<TAllocator, TSparse, TDense, TDenseIndex> : IAttach<TAllocator, TSparse, TDense, TDenseIndex>, IBoolConst
        where TAllocator : unmanaged, IAllocator
        where TSparse : unmanaged
        where TDense : unmanaged, IAttach
        where TDenseIndex : unmanaged
    {
        public bool Is { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => true; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Attach<JSparseBoolConst>(UnmanagedLayout<TAllocator, TSparse, TDense, TDenseIndex>* layout, State state, uint startIndex, uint count)
            where JSparseBoolConst : struct, IBoolConst
        {
            JSparseBoolConst sparseBoolConst = default;
            if (sparseBoolConst.Is)
            {
                var layoutBool = (UnmanagedLayout<TAllocator, bool, TDense, TDenseIndex>*)layout;
                AttachLayoutActions.Attach_bool(ref *layoutBool, state, startIndex);
            }
            else
            {
                var layoutUshort = (UnmanagedLayout<TAllocator, ushort, TDense, TDenseIndex>*)layout;
                AttachLayoutActions.Attach_ushort(ref *layoutUshort, state, count);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Attach<JSparseBoolConst>(UnmanagedLayout<TAllocator, TSparse, TDense, TDenseIndex>* layout, State state, ref NArray<BAllocator, Op> ops)
            where JSparseBoolConst : struct, IBoolConst
        {
            JSparseBoolConst sparseBoolConst = default;
            if (sparseBoolConst.Is)
            {
                var layoutBool = (UnmanagedLayout<TAllocator, bool, TDense, TDenseIndex>*)layout;
                AttachLayoutActions.Attach_bool(ref *layoutBool, state, ref ops);
            }
            else
            {
                var layoutUshort = (UnmanagedLayout<TAllocator, ushort, TDense, TDenseIndex>*)layout;
                AttachLayoutActions.Attach_ushort(ref *layoutUshort, state, ref ops);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Attach(State state, ref TDense component)
        {
            component.OnAttach(state);
        }
    }
}

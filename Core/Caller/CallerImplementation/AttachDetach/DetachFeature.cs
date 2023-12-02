using System.Runtime.CompilerServices;
using AnotherECS.Core.Actions;
using AnotherECS.Core.Collection;

namespace AnotherECS.Core.Caller
{
    internal unsafe struct DetachFeature<TSparse, TDense, TDenseIndex, TTickData> : IDetach<TSparse, TDense, TDenseIndex, TTickData>, IBoolConst
        where TSparse : unmanaged
        where TDense : unmanaged, IDetach
        where TDenseIndex : unmanaged
        where TTickData : unmanaged
    {
        public bool Is { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => true; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Detach<JSparseBoolConst>(UnmanagedLayout<TSparse, TDense, TDenseIndex, TTickData>* layout, State state, uint startIndex, uint count)
            where JSparseBoolConst : struct, IBoolConst
        {
            JSparseBoolConst sparseBoolConst = default;
            if (sparseBoolConst.Is)
            {
                var layoutBool = (UnmanagedLayout<bool, TDense, TDenseIndex, TTickData>*)layout;
                DetachLayoutActions.Detach_bool(ref *layoutBool, state, startIndex);
            }
            else
            {
                var layoutUshort = (UnmanagedLayout<ushort, TDense, TDenseIndex, TTickData>*)layout;
                DetachLayoutActions.Detach_ushort(ref *layoutUshort, state, count);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Detach<JSparseBoolConst>(UnmanagedLayout<TSparse, TDense, TDenseIndex, TTickData>* layout, State state, ref NArray<Op> ops)
            where JSparseBoolConst : struct, IBoolConst
        {
            JSparseBoolConst sparseBoolConst = default;
            if (sparseBoolConst.Is)
            {
                var layoutBool = (UnmanagedLayout<bool, TDense, TDenseIndex, TTickData>*)layout;
                DetachLayoutActions.Detach_bool(ref *layoutBool, state, ref ops);
            }
            else
            {
                var layoutUshort = (UnmanagedLayout<ushort, TDense, TDenseIndex, TTickData>*)layout;
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

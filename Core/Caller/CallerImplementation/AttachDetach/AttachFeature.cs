using System.Runtime.CompilerServices;
using AnotherECS.Core.Actions;
using AnotherECS.Core.Collection;

namespace AnotherECS.Core.Caller
{
    internal unsafe struct AttachFeature<TSparse, TDense, TDenseIndex, TTickData> : IAttach<TSparse, TDense, TDenseIndex, TTickData>, IBoolConst
            where TSparse : unmanaged
            where TDense : unmanaged, IAttach
            where TDenseIndex : unmanaged
            where TTickData : unmanaged
    {
        public bool Is { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => true; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Attach<JSparseBoolConst>(UnmanagedLayout<TSparse, TDense, TDenseIndex, TTickData>* layout, State state, uint startIndex, uint count)
            where JSparseBoolConst : struct, IBoolConst
        {
            JSparseBoolConst sparseBoolConst = default;
            if (sparseBoolConst.Is)
            {
                var layoutBool = (UnmanagedLayout<bool, TDense, TDenseIndex, TTickData>*)layout;
                AttachLayoutActions.Attach_bool(ref *layoutBool, state, startIndex);
            }
            else
            {
                var layoutUshort = (UnmanagedLayout<ushort, TDense, TDenseIndex, TTickData>*)layout;
                AttachLayoutActions.Attach_ushort(ref *layoutUshort, state, count);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Attach<JSparseBoolConst>(UnmanagedLayout<TSparse, TDense, TDenseIndex, TTickData>* layout, State state, ref NArray<Op> ops)
            where JSparseBoolConst : struct, IBoolConst
        {
            JSparseBoolConst sparseBoolConst = default;
            if (sparseBoolConst.Is)
            {
                var layoutBool = (UnmanagedLayout<bool, TDense, TDenseIndex, TTickData>*)layout;
                AttachLayoutActions.Attach_bool(ref *layoutBool, state, ref ops);
            }
            else
            {
                var layoutUshort = (UnmanagedLayout<ushort, TDense, TDenseIndex, TTickData>*)layout;
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

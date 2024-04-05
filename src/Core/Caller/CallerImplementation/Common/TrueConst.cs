using System.Runtime.CompilerServices;

namespace AnotherECS.Core.Caller
{
    internal struct TrueConst : IBoolConst
    {
        public bool Is { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => true; }
    }

    internal struct FalseConst : IBoolConst
    {
        public bool Is { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => false; }
    }
}

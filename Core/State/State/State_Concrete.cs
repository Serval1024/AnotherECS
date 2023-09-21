using System;
using System.Runtime.InteropServices;

namespace AnotherECS.Core
{
    public unsafe class State_Concrete : State
    {
        [StructLayout(LayoutKind.Sequential)]
        public readonly struct FastAccess
        {
            private readonly UnmanagedLayout* _layoutPtr;

            private FastAccess(UnmanagedLayout* layoutPtr)
            {
                _layoutPtr = layoutPtr;
            }

            //public ref SimpleCaller<HA0> HA0;
                //=> ;
        }

        private FastAccess _fast;
        public ref readonly FastAccess Fast => ref _fast;


        protected override void BindingCodeGenerationStage(in GeneralConfig general)
        {
            Install_HA0(this);

            UpdateFastAccess();
        }

        private void UpdateFastAccess()
        {
            FastAccess fastAccess;
            UpdateFastAccess(&fastAccess);
            _fast = fastAccess;
        }

        private static void Install_HA0(State state)   //отдельно
        {
            //state.AddLayout<SimpleCaller<HA0>, HA0>(new ComponentFunction<HA0>() { construct = &HA0_Construct, deconstruct = &HA0_Deconstruct });

            static void HA0_Construct(ref UnmanagedLayout<HA0> layout, ref GlobalDepencies depencies)
            {
            }

            static void HA0_Deconstruct(ref UnmanagedLayout<HA0> layout, ref GlobalDepencies depencies)
            {
            }
        }

        


        protected override uint GetComponentCount()
            => 1;

        protected override ushort GetIndex<T>()
            //=> CompileComponentIdProvider<State_Concrete, T>.ID;
            => 1;

        protected override void OnTickFinished()
        {
            throw new NotImplementedException();
        }



        public struct HA0 : IComponent
        {
            public int f;
        }

        public struct HA1 : IComponent
        {
        }

        public unsafe static void TEST3()
        {

            var size0 = sizeof(UnmanagedLayout);
            var size1 = sizeof(UnmanagedLayout<UnmanagedLayout.Mock>);
            UnityEngine.Debug.Log(size0);
            UnityEngine.Debug.Log(size1);
        }

    }

}


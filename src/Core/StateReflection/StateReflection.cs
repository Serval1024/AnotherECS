using AnotherECS.Converter;
using AnotherECS.Core.Converter;
using AnotherECS.Serializer;
using System;
using System.Linq;

namespace AnotherECS.Core
{
    [IgnoreCompile]
    public sealed class StateReflection : State, IState
    {
        public StateReflection() : base() { }
        public StateReflection(in StateConfig config) : base(in config) { }
        public StateReflection(ref ReaderContextSerializer reader) : base(ref reader) { }

        protected override void BindingCodeGenerationStage(in StateConfig config)
        {
            LayoutInstaller.Install(this);
        }

        protected override uint GetComponentCount()
            => (uint)RuntimeComponentIdStaticProvider<StateReflection>.Instance.GetAssociationTable().Count;

        protected override ushort GetIndex<T>()
            => RuntimeComponentIdProvider<StateReflection, T>.ID;

        protected override uint GetConfigCount()
            => (uint)RuntimeConfigIdStaticProvider<StateReflection>.Instance.GetAssociationTable().Count;

        protected override ushort GetConfigIndex<T>()
            => RuntimeConfigIdProvider<StateReflection, T>.ID;

        protected override ushort GetConfigIndex(Type type)
            => RuntimeConfigIdStaticProvider<StateReflection>.Instance.TypeToId(type);

        protected override ushort GetSignalIndex<T>()
            => RuntimeSignalIdProvider<StateReflection, T>.ID;

        protected override SystemRegisters GetSystemRegisters()
            => new()
            {
                register = ReflectionSystemGlobalRegister.Instance,
                autoAttachRegister = ReflectionSystemAutoAttachGlobalRegister.Instance,
            };

        private static class LayoutInstaller
        {
            public static void Install(State state)
            {
                var installer = ReflectionCommonLayoutInstaller.Create();

                foreach(var component in RuntimeComponentIdStaticProvider<StateReflection>.Instance
                    .GetAssociationTable()
                    .Select(p => p).OrderBy(p => p.Key))
                {
                    installer.Install(state, component.Value);
                }
            }
        }
    }
}
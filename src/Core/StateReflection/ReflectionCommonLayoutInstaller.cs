using AnotherECS.Core.Allocators;
using AnotherECS.Core.Caller;
using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace AnotherECS.Core
{
    internal struct ReflectionCommonLayoutInstaller
    {
        private Type _reflectionComponentFunction;
        private object[] _args;
        private MethodInfo _addLayoutMethod;
        private MethodInfo _addLayoutMethodWithFunction;

        public static ReflectionCommonLayoutInstaller Create()
        {
            ReflectionCommonLayoutInstaller inst = default;
            inst.Init();
            return inst;
        }

        public void Init()
        {
            _reflectionComponentFunction = typeof(ReflectionComponentFunction<>);
            _args = new object[1];

            var methods = typeof(State)
                .GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(p => p.Name == nameof(State.AddLayout))
                .ToArray();

            _addLayoutMethod =
                methods.First(p => p.GetParameters().Length == 0 && p.Name == nameof(State.AddLayout));

            _addLayoutMethodWithFunction =
                methods.First(p => p.GetParameters().Length == 1 && p.Name == nameof(State.AddLayout));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Install(State state, Type component)
        {
            var typeOptions = new TypeOptions(component);

            var callerType = StateReflectionHelper.MakeTypeByDeclaration(
                CallerDeclaration.GetCallerDeclaration(typeOptions)
                );

            if (typeOptions.IsUseComponentFunction)
            {
                var argument = _reflectionComponentFunction.MakeGenericType(component);

                var getter = argument.GetMethods(BindingFlags.Public | BindingFlags.Static)[0];
                _args[0] = getter.Invoke(null, null);

                _addLayoutMethodWithFunction
                    .MakeGenericMethod(callerType, component)
                    .Invoke(state, _args);
            }
            else
            {
                _addLayoutMethod
                    .MakeGenericMethod(callerType, component)
                    .Invoke(state, null);
            }
        }


        private class ReflectionComponentFunction<TComponent>
            where TComponent : unmanaged, IComponent
        {
            public static ComponentFunction<TComponent> GetComponentFunction()
                => new() { 
                    construct = &Construct, 
                    deconstruct = &Deconstruct, 
                    repairMemory = &RepairMemory,
                    repairStateId = &RepairStateId
                };

            static void Construct(ref InjectContainer injectContainer, ref TComponent component)
            {
                ComponentReflectionUtils.Construct(ref component, ref injectContainer);
            }

            static void Deconstruct(ref InjectContainer injectContainer, ref TComponent component)
            {
                ComponentReflectionUtils.Deconstruct(ref component, ref injectContainer);
            }

            static void RepairMemory(ref RepairMemoryContext repairMemoryContext, ref TComponent component)
            {
                ComponentReflectionUtils.RepairMemoryHandle(ref component, ref repairMemoryContext);
            }

            static void RepairStateId(ushort stateId, ref TComponent component)
            {
                ComponentReflectionUtils.RepairStateId(ref component, stateId);
            }
        }
    }
}

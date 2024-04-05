using AnotherECS.Core.Allocators;
using AnotherECS.Core.Collection;

namespace AnotherECS.Core
{
    public static class ComponentReflectionUtils
    {
        public static void Construct<T>(ref T component, ref InjectContainer injectContainer)
           where T : struct
           => ReflectionUtils.ReflectionInject(ref component, ref injectContainer, nameof(IInject<WPtr<HAllocator>>.Construct));

        public static void Deconstruct<T>(ref T component, ref InjectContainer injectContainer)
            where T : struct
            => ReflectionUtils.ReflectionInject(ref component, ref injectContainer, nameof(IInject.Deconstruct));

        public static void RepairMemoryHandle<T>(ref T component, ref RepairMemoryContext repairMemoryContext)
            where T : struct
            => ReflectionUtils.ReflectionRepairMemoryHandle(ref component, ref repairMemoryContext);

        public static void RepairStateId<T>(ref T component, ushort stateId)
            where T : struct
            => ReflectionUtils.ReflectionRepairStateId(ref component, stateId);
    }
}
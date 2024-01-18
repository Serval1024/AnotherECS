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

        public static void RebindMemoryHandle<T>(ref T component, ref MemoryRebinderContext rebinder)
            where T : struct
            => ReflectionUtils.ReflectionRebindMemoryHandle<T>(ref component, ref rebinder);
    }
}
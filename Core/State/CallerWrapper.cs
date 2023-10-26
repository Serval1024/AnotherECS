namespace AnotherECS.Core
{
    internal static unsafe class CallerWrapper
    {
        public static void Config<TCaller, TComponent>(ref TCaller caller, UnmanagedLayout* layout, GlobalDepencies* depencies, ushort id, State state)
            where TComponent : unmanaged
            where TCaller : ICaller<TComponent>
        {
            caller.Config(layout, depencies, id, state);
        }

        public static void AllocateLayout<TCaller, TComponent>(ref TCaller caller)
            where TComponent : unmanaged
            where TCaller : ICaller<TComponent>
        {
            caller.AllocateLayout();
        }
    }
}

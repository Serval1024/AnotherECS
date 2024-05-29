using AnotherECS.Core.Remote;
using System;
using System.Linq;

namespace AnotherECS.Core
{
    public static class WorldHelper
    {
        public static ISystem[] FlattenSystems(World world)
        {
            var worldData = world.WorldData;

#if !ANOTHERECS_RELEASE
            var container = new WorldDIContainer(worldData.State);
#else
            var container = new WorldDIContainer(worldData.State, SystemGlobalRegister.GetInjects());
#endif
            var systemRegister = worldData.State.GetSystemData().register;

            worldData.Systems.Sort(systemRegister);
            var context = new InstallContext(world);
            container.Inject(worldData.Systems.GetSystemsAll());

            worldData.Systems.Install(ref context);
            if (context.IsAny())
            {
                worldData.Systems.Append(context.GetSystemGroup());
                worldData.Systems.Sort(systemRegister);
            }
            var systems = worldData.Systems.GetSystemsAll().ToArray();
            container.Inject(systems);

            return systems;
        }

        public static void AutoAttachSystems(ref WorldData worldData)
        {
            if (worldData.IsOneGateAutoAttach)
            {
                foreach (var system in worldData.State.GetSystemData().autoAttachRegister.Gets())
                {
                    worldData.Systems.Prepend((ISystem)Activator.CreateInstance(system));
                }
            }
        }
    }
}

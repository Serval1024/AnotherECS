using AnotherECS.Core;

namespace AnotherECS.Unity.Jobs
{
    [ModuleAutoAttach]
    public class JobsModule : IModule, ICreateModule, IDestroyModule
    {
        public void OnCreateModule(State state)
        {
            JobsGlobalRegister.Register(state);
        }

        public void OnDestroyModule(State state)
        {
            JobsGlobalRegister.Unregister(state);
        }
    }
}



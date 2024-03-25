using AnotherECS.Core;

namespace AnotherECS.Unity.Jobs
{
    [ModuleAutoAttach]
    public class JobsModule : IModule, IAttachToStateModule, IDetachToStateModule
    {
        public void OnAttachToStateModule(State state)
        {
            JobsGlobalRegister.Register(state);
        }

        public void OnDetachToStateModule(State state)
        {
            JobsGlobalRegister.Unregister(state);
        }
    }
}



using AnotherECS.Core;

namespace AnotherECS.Unity.Jobs
{
    [ModuleAutoAttach]
    public class JobsModule : IModule, ICreateModule
    {
        public void OnCreateModule(State state)
        {
            state.SetModuleData(NativeArrayHandles.MODULE_DATA_ID, new NativeArrayHandles(state));
        }
    }
}

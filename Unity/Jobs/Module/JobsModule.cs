using AnotherECS.Core;

namespace AnotherECS.Unity.Jobs
{
    [ModuleAutoAttach]
    public class JobsModule : IModule, IConstructModule
    {
        public void Construct(State state)
        {
            state.SetModuleData(NativeArrayHandles.USER_DATA_ID, new NativeArrayHandles(state));
        }
    }
}
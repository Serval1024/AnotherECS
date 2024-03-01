using AnotherECS.Core;

namespace AnotherECS.Unity.Debug.Diagnostic
{
    [SystemOrder(SystemOrder.Last)]
    public struct DiagnosticModule : IFeature, ITickFinishedModule, IDestroySystem, IMainThread
    {
        private UnityDiagnostic _unityDiagnostic;
        private World _world;

        public void Install(ref InstallContext context)
        {
            context.AddSystem(new CheatFeature());

            _world = context.World;
            _unityDiagnostic.Attach(_world);
        }

        public void OnTickFinished(State state)
        {
            _unityDiagnostic.Update(_world);
        }

        public void OnDestroy(State state)
        {
            _unityDiagnostic.Detach(_world);
        }
    }
}
using AnotherECS.Core;

namespace AnotherECS.Unity.Debug.Diagnostic
{
    [SystemOrder(SystemOrder.First)]
    public struct CheatFeature : IFeature
    {
        public void Install(ref InstallContext context)
        {
            context.AddSystem(new CheatSystem());
        }
    }
}
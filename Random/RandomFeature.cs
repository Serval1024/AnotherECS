using AnotherECS.Core;

namespace AnotherECS.Random
{
    [SystemOrder(SystemOrder.First)]
    public struct RandomFeature : IFeature
    {
        public void Install(ref InstallContext context) 
        {
            var rand = new Mathematics.Random();
            rand.InitState();
            context.AddSingle(new DataRandom() { value = rand });
        }
    }
}
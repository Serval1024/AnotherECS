namespace AnotherECS.Core.Remote
{
    public class RemoveWorldModuleData : IModuleData
    {
        public const uint MODULE_DATA_ID = 2;

        internal Player localPlayer;
        internal double deltaTime;
        internal double time;
    }
}
using AnotherECS.Core;

namespace AnotherECS.Views.Core
{
    public class ViewSystemReference : IModuleData
    {
        public const uint MODULE_DATA_ID = 1;

        internal IViewSystem module;
    }
}

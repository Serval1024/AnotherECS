using System.Runtime.CompilerServices;

namespace AnotherECS.Core.Remote
{
    public static class RemoteStateExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Player GetPlayer(this State state)
        {
#if !ANOTHERECS_RELEASE
            if (!state.IsHasModuleData(RemoveWorldModuleData.MODULE_DATA_ID))
            {
                throw new Core.Exceptions.FeatureNotExists(nameof(RemoteWorld));
            }
#endif
            return state.GetModuleData<RemoveWorldModuleData>(RemoveWorldModuleData.MODULE_DATA_ID).localPlayer;
        }
    }
}
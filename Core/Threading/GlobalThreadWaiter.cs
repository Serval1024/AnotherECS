using System.Runtime.CompilerServices;

namespace AnotherECS.Core.Threading
{
    public class GlobalThreadWaiter
    {        
        private static ThreadWaitProvider _impl = ThreadWaitProvider.Create();

#if UNITY_EDITOR
        [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ReloadDomainOptimizationHack()
        {
            _impl = ThreadWaitProvider.Create();
        }
#endif

        public static int Register(ISystemProcessing processing)
           => _impl.Register(processing);

        public static void Unregister(int id)
            => _impl.Unregister(id);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WaitOtherAndPassOne(int processingId)
        {
            _impl.Wait(processingId);
        }
    }
}

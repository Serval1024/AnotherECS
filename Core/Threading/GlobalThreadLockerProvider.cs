using System.Runtime.CompilerServices;

namespace AnotherECS.Core.Threading
{
    public class GlobalThreadLockerProvider
    {
        private const int _CAPACITY = 16;

        private static ThreadLockerProvider _impl = new(_CAPACITY);

#if UNITY_EDITOR
        [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ReloadDomainOptimizationHack()
        {
            _impl = new(_CAPACITY);
        }
#endif
        public static int AllocateId()
            => _impl.AllocateId();

        public static void DeallocateId(int id)
            => _impl.DeallocateId(id);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object GetLocker(int id)
           => _impl.GetLocker(id);
    }
}

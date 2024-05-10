namespace AnotherECS.Core
{
    public static class SystemAutoAttachGlobalRegister
    {
        private static SystemAutoAttachRegister _instance = null;
        private static readonly object _locker = new();

        public static SystemAutoAttachRegister Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_locker)
                    {
                        _instance ??= new();
                    }
                }
                return _instance;
            }
        }

#if UNITY_EDITOR
        [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ReloadDomainOptimizationHack()
        {
            _instance = null;
        }
#endif
    }
}



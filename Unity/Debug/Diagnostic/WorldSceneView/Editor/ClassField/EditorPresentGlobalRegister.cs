using System;
using System.Linq;

namespace AnotherECS.Unity.Debug.Diagnostic.Editor
{
    internal static class EditorPresentGlobalRegister
    {
        private static IPresent[] _insts;

#if UNITY_EDITOR
        [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ReloadDomainOptimizationHack()
        {
            _insts = null;
        }
#endif

        public static IPresent[] Gets()
            => _insts ??= TypeUtils.GetRuntimeTypes<IPresent>()
                .Select(p => Activator.CreateInstance(p))
                .Cast<IPresent>()
                .ToArray();

        public static IPresent Get(Type type)
            => Gets().FirstOrDefault(p => type.IsAssignableFrom(p.Type));
    }
}


using System;
using System.Linq;

namespace AnotherECS.Unity.Debug.Diagnostic.Present
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
        {
            var variants = Gets()
                .Where(p => p.Type.IsAssignableFrom(type))
                .OrderByDescending(p => p.Priority)
                .ToArray();

            for (int i = 0; i < variants.Length; ++i)
            { 
                int count = 0;
                for (int j = i + 1; j < variants.Length; ++j)
                {
                    if (variants[i].Type.IsAssignableFrom(variants[j].Type))
                    {
                        ++count;
                        break;
                    }
                }
                if (count == 0)
                {
                    return variants[i];
                }
            }
            return null;
        }
    }
}


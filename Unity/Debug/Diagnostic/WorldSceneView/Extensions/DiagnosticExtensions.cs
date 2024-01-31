using AnotherECS.Core;

namespace AnotherECS.Unity.Debug.Diagnostic
{
    public static class DiagnosticExtensions
    {
        public static void EnableUnityDiagnostic(this World world)
        {
            world.SetDiagnostic(new UnityDiagnostic());
        }
    }
}

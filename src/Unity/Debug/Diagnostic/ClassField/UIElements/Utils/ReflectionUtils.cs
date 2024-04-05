namespace AnotherECS.Debug.Diagnostic.UIElements
{
    internal class ReflectionUtils
    {
        public const System.Reflection.BindingFlags nonPublicFlags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
        public const System.Reflection.BindingFlags publicFlags = System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance;
        public const System.Reflection.BindingFlags instanceFlags = nonPublicFlags | publicFlags;
    }
}
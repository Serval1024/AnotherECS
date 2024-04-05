namespace AnotherECS.Unity.Debug.Diagnostic.Editor
{
    public static class DisplayUtils
    {
        public static string ToStringMemory(ulong value)
        {
            const ulong STEP = 1024;

            if (value < STEP)
            {
                return $"{value}b";
            }
            else if (value < STEP * STEP)
            {
                return $"{value / STEP}kb";
            }
            return $"{value / (STEP * STEP)}mb";
        }
    }
}

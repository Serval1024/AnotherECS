namespace AnotherECS.Core
{
    internal static class InjectContextUtils
    {
        public static void PrepareContext(ref InjectContext injectContext, AllocatorType allocatorType)
        {
            injectContext.variables.Clear();
            injectContext.variables.Add("allocatorType", (int)allocatorType);
        }
    }
}
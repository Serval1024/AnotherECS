namespace AnotherECS.Unity.Jobs
{
#if ENABLE_IL2CPP
    [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.NullChecks, false)]
    [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false)]
    [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
#endif
    internal static class JobBagUtils
    {
        public static int GetScheduleBatchCount(int count)
        {
            const int batch = 64;

            var batchCount = count / batch;
            if (batchCount == 0) batchCount = 1;
            if (count <= 10 && batchCount == 1)
            {

                return batchCount;

            }
            else if (batchCount == 1)
            {

                batchCount = 2;

            }

            return batchCount;
        }
    }
}
    

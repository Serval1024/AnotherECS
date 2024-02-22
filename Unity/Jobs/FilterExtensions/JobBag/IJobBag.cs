namespace AnotherECS.Unity.Jobs
{
    public interface IJobBag
    {
        int Count { get; }
        void BeginForEachIndex(int chunkIndex);
        void EndForEachIndex();
    }
}

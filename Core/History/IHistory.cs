namespace AnotherECS.Core
{
    [Serializer.Serialize()]
    public interface IHistory { }

    internal interface IPoolHistory : IHistory { }
    internal interface IRevert 
    {
        void RevertTo(uint tick);
    }
}
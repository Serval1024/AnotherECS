namespace AnotherECS.Core
{
    [Serializer.Serialize()]
    public interface IHistory { }
    
    internal interface IRevert 
    {
        void RevertTo(uint tick);
    }
}
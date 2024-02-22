namespace AnotherECS.Core.Remote
{
    public interface IHubBytesProvider
    {
        ChildHubProvider Get(uint worldId);
    }
}

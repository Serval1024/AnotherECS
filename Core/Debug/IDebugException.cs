using EntityId = System.UInt32;

namespace AnotherECS.Core
{
    public interface IDebugException 
    {
        bool IsDisposed { get; }
        bool IsHas(EntityId id);
    }
}

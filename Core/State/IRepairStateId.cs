namespace AnotherECS.Core
{
    public interface IRepairStateId
    {
        bool IsRepairStateId { get; }
        void RepairStateId(ushort stateId);
    }
}
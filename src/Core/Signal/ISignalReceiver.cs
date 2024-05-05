namespace AnotherECS.Core
{
    public interface ISignalReceiver
    {
        void OnFire(ISignal signal);
        void OnCancel(ISignal signal);
        void OnLeaveHistoryBuffer(ISignal signal);
    }

    public interface ISignalReceiver<TSignal> : ISignalReceiver
        where TSignal : ISignal
    {
        void ISignalReceiver.OnFire(ISignal signal)
            => OnFire((TSignal) signal);

        void ISignalReceiver.OnCancel(ISignal signal)
            => OnCancel((TSignal)signal);

        void ISignalReceiver.OnLeaveHistoryBuffer(ISignal signal)
            => OnLeaveHistoryBuffer((TSignal)signal);

        void OnFire(TSignal signal);
        void OnCancel(TSignal signal);
        void OnLeaveHistoryBuffer(TSignal signal);
    }
}
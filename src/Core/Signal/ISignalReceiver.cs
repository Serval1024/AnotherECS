namespace AnotherECS.Core
{
    public interface ISignalReceiver
    {
        void OnFire(ISignal signal);
        void OnCancel(ISignal signal);
        void OnHistoryBufferLeaved(ISignal signal);
    }

    public interface ISignalReceiver<TSignal> : ISignalReceiver
        where TSignal : ISignal
    {
        void ISignalReceiver.OnFire(ISignal signal)
            => OnFire((TSignal) signal);

        void ISignalReceiver.OnCancel(ISignal signal)
            => OnCancel((TSignal)signal);

        void ISignalReceiver.OnHistoryBufferLeaved(ISignal signal)
            => OnHistoryBufferLeaved((TSignal)signal);

        void OnFire(TSignal signal);
        void OnCancel(TSignal signal);
        void OnHistoryBufferLeaved(TSignal signal);
    }
}
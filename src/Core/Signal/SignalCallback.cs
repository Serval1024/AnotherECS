namespace AnotherECS.Core
{
    public struct SignalCallback
    {
        public CommandType Command;
        public ISignal Signal;

        public SignalCallback(CommandType command, ISignal signal)
        {
            Command = command;
            Signal = signal;
        }

        public enum CommandType
        {
            None = 0,
            Fire,
            Cancel,
            LeaveBuffer,
        }
    }
}


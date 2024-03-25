namespace AnotherECS.Debug
{
    public interface ILogger
    {
        void Send(string message);
        void Error(string message);
    }
}

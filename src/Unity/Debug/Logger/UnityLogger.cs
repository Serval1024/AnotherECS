using AnotherECS.Debug;

namespace AnotherECS.Unity.Debug
{
    public class UnityLogger : ILogger
    {
        public void Send(string message)
        {
            UnityEngine.Debug.Log(message);
        }

        public void Error(string message)
        {
            UnityEngine.Debug.LogError(message);
        }
    }
}

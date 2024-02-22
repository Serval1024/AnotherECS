
namespace AnotherECS.Debug
{
    public static class Logger
    {
        public static void RevertStateFail(string error)
            => UnityEngine.Debug.Log($"{DebugConst.TAG}Failed to revert state: '{error}'.");

        public static void ReceiveCorruptedData(string error)
            => UnityEngine.Debug.Log($"{DebugConst.TAG}Received corrupted data from the network: '{error}'.");

        public static void Send(string message)
            => UnityEngine.Debug.Log(message);

        public static void FileDeleted(string path)
            => Send($"{DebugConst.TAG}File deleted: '{path}'.");

        public static void CompileFinished()
            => Send($"{DebugConst.TAG}Compile finished.");

        public static void CompileFailed()
            => Send($"{DebugConst.TAG}Compile failed.");

        public static void HistoryBufferResized(string name, uint newSize)
            => Send($"{DebugConst.TAG}History {name} buffer size has been resized to {newSize}.");
    }
}

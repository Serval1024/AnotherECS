namespace AnotherECS.Debug
{
    public static class Logger
    {
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

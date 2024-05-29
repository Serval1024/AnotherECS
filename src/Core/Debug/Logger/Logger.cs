using AnotherECS.Converter;
using System.Linq;

namespace AnotherECS.Debug
{
    public static class Logger
    {
        private static bool _isOneGate = true;
        private static ILogger _impl;
        private static readonly object _locker = new();

        private static ILogger Impl
        {
            get
            {
                lock (_locker)
                {
                    if (_isOneGate)
                    {
                        _isOneGate = false;

                        var type = TypeUtils.GetRuntimeTypes<ILogger>().FirstOrDefault();
                        if (type != null)
                        {
                            _impl = (ILogger)System.Activator.CreateInstance(type);
                        }
                    }
                    return _impl;
                }
            }
        }

        public static void Send(string message)
        {
            Impl?.Send($"{DebugConst.TAG}{message}");
        }

        public static void Error(string message)
        {
            Impl?.Error($"{DebugConst.TAG}{message}");
        }

        public static void RevertStateFail(string error)
        {
            Error($"Failed to revert state: '{error}'.");
        }

        public static void ReceiveCorruptedData(string error)
        {
            Error($"Received corrupted data from the network: '{error}'.");
        }

        public static void FileDeleted(string path)
        {
            Send($"File deleted: '{path}'.");
        }

        public static void CompileFinished()
        {
            Send($"Compile finished.");
        }

        public static void CompileFailed()
        {
            Error($"Compile failed.");
        }

        public static void HistoryBufferDataResized(uint newSize)
        {
#if UNITY_EDITOR || !UNITY_5_3_OR_NEWER
            Send($"{DebugConst.TAG}{$"History 'Data' buffer size has been resized to '{newSize}'"}.");
#endif
        }

        public static void HistoryBufferMetaResized(uint newSize)
        {
#if UNITY_EDITOR || !UNITY_5_3_OR_NEWER
            Send($"{DebugConst.TAG}{$"History 'Meta' buffer size has been resized to '{newSize}'"}.");
#endif
        }
    }
}

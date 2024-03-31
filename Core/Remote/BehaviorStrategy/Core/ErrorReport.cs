using System;

namespace AnotherECS.Core.Remote
{
    public readonly struct ErrorReport
    {
        public Exception Exception { get; }

        public ErrorReport(Exception ex)
        {
            Exception = ex;
        }

        public bool Is<T>()
            => Exception != null && typeof(T).IsAssignableFrom(Exception.GetType());
    }
}

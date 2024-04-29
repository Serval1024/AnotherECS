using System;

namespace AnotherECS.Core.Remote
{
    public static class ExceptionExtensions
    {
        public static Exception GetRoot(this AggregateException aggregateException)
        {
            Exception ex = aggregateException;
            while (ex is AggregateException && ex.InnerException != null)
            {
                ex = ex.InnerException;
            }
            return ex;
        }
    }
}

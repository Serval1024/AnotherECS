using System;

namespace AnotherECS.Core.Remote
{
    public static class ExceptionHelper
    {
        public static Exception ExtractRootException(AggregateException aggregateException)
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

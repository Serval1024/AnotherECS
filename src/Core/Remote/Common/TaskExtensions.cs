using System;
using System.Threading.Tasks;

namespace AnotherECS.Core.Remote
{
    public static class TaskExtensions
    {
        public static Task<TResult> Run<TResult>(this Func<Task<TResult>> function)
        {
            if (function == null)
            {
                throw new ArgumentNullException(nameof(function));
            }

            return Task<Task<TResult>>.Factory.StartNew(function).Unwrap();
        }

        public static Task<TResult> Run<TResult>(this Func<object, Task<TResult>> function, object state)
        {
            if (function == null)
            {
                throw new ArgumentNullException(nameof(function));
            }

            return Task<Task<TResult>>.Factory.StartNew(function, state).Unwrap();
        }
    }
}
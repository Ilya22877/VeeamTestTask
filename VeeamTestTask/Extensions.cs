using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace VeeamTestTask
{
    public static class Extensions
    {
        public static void WaitAll(this IEnumerable<Thread> threads)
        {
            foreach (var thread in threads)
            {
                thread.Join();
            }
        }

        public static void ThrowIfNotEmpty(this IReadOnlyCollection<Exception> errors)
        {
            if (errors.Any())
            {
                throw new AggregateException(errors);
            }
        }
    }
}
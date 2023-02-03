using System;
using System.Threading.Tasks;
using Akka.Util;
using JetBrains.Annotations;

namespace TimeTracker
{
    [PublicAPI]
    public static class Try
    {
        public static Try<TType> From<TType>(Func<TType> func)
            => Try<TType>.From(func);

        public static async Task<Try<TType>> FromAsync<TType>(Func<Task<TType>> func)
        {
            try
            {
                return new Try<TType>(await func().ConfigureAwait(false));
            }
            catch (Exception e)
            {
                return new Try<TType>(e);
            }
        }
    }
}
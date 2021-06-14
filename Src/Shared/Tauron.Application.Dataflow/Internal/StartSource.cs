using System;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Tauron.Application.Dataflow.Internal
{
    internal sealed class StartSource
    {
        public static ISourceBlock<TOutput> Create<TOutput>(Func<TOutput> dataFunc)
        {
            var block = new WriteOnceBlock<TOutput>(d => d);
            Task.Run(async () =>
                     {
                         await block.SendAsync(dataFunc());
                     });
            return block;
        }

        public static ISourceBlock<TOutput> Create<TOutput>(Func<Task<TOutput>> dataFunc)
        {
            var block = new WriteOnceBlock<TOutput>(d => d);
            Task.Run(async () =>
                     {
                         await block.SendAsync(await dataFunc());
                     });
            return block;
        }
    }
}
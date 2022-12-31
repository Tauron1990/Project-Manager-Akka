using Tauron.Application.Workshop.Mutation;

namespace Tauron.Application.Workshop.Driver;

public sealed class MutationRunner
{
    private readonly EventLoopExecutor _eventLoopExecutor = new();

    public void Enqueue(IDataMutation mut)
        => _eventLoopExecutor.Enqueue(
            async () =>
            {
                switch (mut)
                {
                    case ISyncMutation sync:
                        sync.Run();

                        break;
                    case IAsyncMutation asyncMutation:
                        await asyncMutation.Run().ConfigureAwait(false);

                        break;
                }
            });

    // ReSharper restore InconsistentlySynchronizedField
}
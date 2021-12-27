using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using Tauron.Application.Workshop.Mutation;

namespace Tauron.Application.Workshop.Driver;

public sealed class MutationRunner
{
    private readonly ConcurrentQueue<IDataMutation> _mutations = new();
    private int _running;

    // ReSharper disable once CognitiveComplexity
    private async Task RunMutaions()
    {
        do
        {
            // ReSharper disable InconsistentlySynchronizedField
            if (_mutations.TryDequeue(out var mut))
            {
                switch (mut)
                {
                    case ISyncMutation syncMutation:
                        syncMutation.Run();
                        break;
                    case IAsyncMutation asyncMutation:
                        await asyncMutation.Run();
                        break;
                }
                
                if(RuntimeInformation.ProcessArchitecture == Architecture.Wasm)
                    await Task.Yield();
            }
            else
            {
                lock (_mutations)
                {
                    if (!_mutations.IsEmpty) continue;

                    _running = 0;
                    break;
                }
            }
        } while (true);
    }

    public void Enqueue(IDataMutation mut)
    {
        _mutations.Enqueue(mut);

        lock (_mutations)
        {
            if(_running == 1) return;

            _running = 1;
            Task.Run(RunMutaions);
        }
    }
    
    // ReSharper restore InconsistentlySynchronizedField
}
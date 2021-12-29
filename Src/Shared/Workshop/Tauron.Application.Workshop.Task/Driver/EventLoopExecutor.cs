using System.Collections.Concurrent;
using System.Runtime.InteropServices;

namespace Tauron.Application.Workshop.Driver;

public sealed class EventLoopExecutor
{
    private readonly ConcurrentQueue<Func<Task>> _mutations = new();
    private readonly bool _needYield;
    private int _running;

    public EventLoopExecutor()
        => _needYield = RuntimeInformation.ProcessArchitecture == Architecture.Wasm && Environment.Version.Major <= 6;

    // ReSharper disable once CognitiveComplexity
    private async Task RunMutaions()
    {
        do
        {
            // ReSharper disable InconsistentlySynchronizedField
            if (_mutations.TryDequeue(out var mut))
            {
                await mut();
                
                if(_needYield)
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

    public void Enqueue(Func<Task> mut)
    {
        _mutations.Enqueue(mut);

        lock (_mutations)
        {
            if(_running == 1) return;

            _running = 1;
            Task.Run(RunMutaions);
        }
    }
}
using Tauron.Application.Workshop.Driver;

namespace Tauron.Application.Workshop.Processor;

internal sealed class ProcesRunner : IProcessorQueue
{
    private readonly EventLoopExecutor _executor = new();
    
    public void Run(Func<Task> itemToRun)
        => _executor.Enqueue(itemToRun);
}
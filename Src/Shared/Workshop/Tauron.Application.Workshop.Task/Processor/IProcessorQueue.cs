namespace Tauron.Application.Workshop.Processor;

public interface IProcessorQueue
{
    void Run(Func<Task> itemToRun);
}
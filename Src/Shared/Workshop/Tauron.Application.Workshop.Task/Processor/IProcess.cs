namespace Tauron.Application.Workshop.Processor;

public interface IProcess
{
    void Init(IProcessorQueue queue);
}
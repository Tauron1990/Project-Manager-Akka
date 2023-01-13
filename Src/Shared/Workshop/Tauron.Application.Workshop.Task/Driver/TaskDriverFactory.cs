using Microsoft.Extensions.DependencyInjection;
using Tauron.Application.Workshop.Analyser.Analyser;
using Tauron.Application.Workshop.Analyzing;
using Tauron.Application.Workshop.Mutation;
using Tauron.Application.Workshop.Processor;
using Tauron.Operations;

namespace Tauron.Application.Workshop.Driver;

public sealed class TaskDriverFactory : IDriverFactory, IDisposable
{
    private readonly List<IProcess> _processes = new();
    private readonly IProcessorQueue _processorQueue = new ProcesRunner();
    private readonly IServiceProvider _provider;

    public TaskDriverFactory(IServiceProvider provider)
        => _provider = provider;

    public void Dispose()
        // ReSharper disable once SuspiciousTypeConversion.Global
        => _processes.OfType<IDisposable>().Foreach(d => d.Dispose());

    public Action<IDataMutation> CreateMutator()
        => new MutationRunner().Enqueue;

    public Action<RegisterRule<TWorkspace, TData>> CreateAnalyser<TWorkspace, TData>(TWorkspace workspace, IObserver<RuleIssuesChanged<TWorkspace, TData>> observer)
        where TWorkspace : WorkspaceBase<TData> where TData : class
        => new DirectAnalyser<TWorkspace, TData>(observer, workspace).Register;

    public void CreateProcessor(Type processor, string name)
    {
        if(!processor.IsAssignableTo(typeof(IProcess)))
            throw new InvalidOperationException("The Type is not a Valid Process");

        var processInst = (IProcess)ActivatorUtilities.CreateInstance(_provider, processor);
        processInst.Init(_processorQueue);
        _processes.Add(processInst);
    }

    public Action<IOperationResult>? GetResultSender()
        => null;
}
using Tauron.Application.Workshop.Analyser.Analyser;
using Tauron.Application.Workshop.Analyzing;
using Tauron.Application.Workshop.Mutation;

namespace Tauron.Application.Workshop.Driver;

public sealed class TaskDriverFactory : IDriverFactory
{
    public Action<IDataMutation> CreateMutator()
        => new MutationRunner().Enqueue;

    public Action<RegisterRule<TWorkspace, TData>> CreateAnalyser<TWorkspace, TData>(TWorkspace workspace, IObserver<RuleIssuesChanged<TWorkspace, TData>> observer)
        where TWorkspace : WorkspaceBase<TData> where TData : class
        => new DirectAnalyser<TWorkspace, TData>(observer, workspace).Register;
}
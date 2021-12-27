using System;
using Tauron.Application.Workshop.Analyzing;
using Tauron.Application.Workshop.Mutation;

namespace Tauron.Application.Workshop.Driver;

public interface IDriverFactory
{
    Action<IDataMutation> CreateMutator();

    Action<RegisterRule<TWorkspace, TData>> CreateAnalyser<TWorkspace, TData>(
        TWorkspace workspace, IObserver<RuleIssuesChanged<TWorkspace, TData>> observer) 
        where TData : class where TWorkspace : WorkspaceBase<TData>;
}
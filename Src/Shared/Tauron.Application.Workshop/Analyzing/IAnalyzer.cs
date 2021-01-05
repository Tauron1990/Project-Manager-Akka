using JetBrains.Annotations;
using Tauron.Application.Workshop.Analyzing.Rules;
using Tauron.Application.Workshop.Mutation;

namespace Tauron.Application.Workshop.Analyzing
{
    [PublicAPI]
    public interface IAnalyzer
    {
        IEventSource<IssuesEvent> Issues { get; }
    }

    [PublicAPI]
    public interface IAnalyzer<out TWorkspace, TData> : IAnalyzer
        where TWorkspace : WorkspaceBase<TData> where TData : class
    {
        void RegisterRule(IRule<TWorkspace, TData> rule);
    }
}
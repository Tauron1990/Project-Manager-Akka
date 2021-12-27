using System.Collections.Immutable;
using Tauron.Application.Workshop.Analyzing;
using Tauron.Application.Workshop.Analyzing.Rules;

namespace Tauron.Application.Workshop.Analyser.Analyser;

public sealed class DirectAnalyser<TWorkspace, TData>
    where TData : class where TWorkspace : WorkspaceBase<TData>
{
    private readonly object _lock = new();
    private readonly IObserver<RuleIssuesChanged<TWorkspace, TData>> _observer;
    private readonly TWorkspace _workspace;
    private ImmutableList<IRule<TWorkspace, TData>> _rules = ImmutableList<IRule<TWorkspace, TData>>.Empty;

    public DirectAnalyser(IObserver<RuleIssuesChanged<TWorkspace, TData>> observer, TWorkspace workspace)
    {
        _observer = observer;
        _workspace = workspace;
    }

    public void Register(RegisterRule<TWorkspace, TData> register)
    {
        register.Rule.Init(new Action<RuleIssuesChanged<TWorkspace, TData>>(_observer.OnNext), _workspace);
        
        lock (_lock)
            _rules = _rules.Add(register.Rule);
    }
}
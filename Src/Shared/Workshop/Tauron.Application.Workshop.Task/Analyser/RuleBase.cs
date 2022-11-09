using JetBrains.Annotations;
using Tauron.Application.Workshop.Analyzing;
using Tauron.Application.Workshop.Analyzing.Rules;

namespace Tauron.Application.Workshop.Analyser;

[PublicAPI]
public abstract class RuleBase<TWorkshop, TData> : IRule<TWorkshop, TData>
    where TWorkshop : WorkspaceBase<TData> where TData : class
{
    private Action<RuleIssuesChanged<TWorkshop, TData>> _sender = _ => { };

    public RuleBase(string name)
        => Name = name;

    public string Name { get; }

    public void Init(object metadata, TWorkshop workspace)
    {
        if(metadata is not Action<RuleIssuesChanged<TWorkshop, TData>> sender)
            throw new InvalidOperationException("Only Action Sender are Supportet for Task Rule");

        _sender = sender;
        InitRule(workspace);
    }

    protected abstract void InitRule(TWorkshop workshop);

    protected void SendIssues(IEnumerable<Issue.IssueCompleter> issues)
        => _sender(new RuleIssuesChanged<TWorkshop, TData>(this, issues));
}
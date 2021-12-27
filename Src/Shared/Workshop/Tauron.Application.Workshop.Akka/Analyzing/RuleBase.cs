using Akka.Actor;
using JetBrains.Annotations;
using Tauron.Application.Workshop.Analyzing.Rules;
using Tauron.TAkka;

namespace Tauron.Application.Workshop.Analyzing;

[PublicAPI]
public abstract class RuleBase<TWorkspace, TData> : IRule<TWorkspace, TData>
    where TWorkspace : WorkspaceBase<TData> where TData : class
{
    protected TWorkspace Workspace { get; private set; } = null!;

    public abstract string Name { get; }

    public void Init(object metadata, TWorkspace workspace)
    {
        if (metadata is not IActorRefFactory factory)
            throw new InvalidOperationException("Actor Rule Base is Only Compatible with Actor Analyser");
        
        Workspace = workspace;

        factory.ActorOf(() => new InternalRuleActor(ActorConstruct), Name);
    }

    protected abstract void ActorConstruct(IObservableActor actor);

    protected void SendIssues(IEnumerable<Issue.IssueCompleter> issues, IActorContext context)
    {
        context.Parent.Tell(new RuleIssuesChanged<TWorkspace, TData>(this, issues));
    }

    private sealed class InternalRuleActor : ObservableActor
    {
        internal InternalRuleActor(Action<IObservableActor> constructor) => constructor(this);
    }
}
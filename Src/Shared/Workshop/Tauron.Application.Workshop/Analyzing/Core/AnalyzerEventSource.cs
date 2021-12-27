using System;
using System.Reactive;
using Tauron.Application.Workshop.Mutation;

namespace Tauron.Application.Workshop.Analyzing.Core;

public sealed class AnalyzerEventSource<TWorkspace, TData> : EventSourceBase<IssuesEvent>
    where TWorkspace : WorkspaceBase<TData> where TData : class
{
    public IObserver<RuleIssuesChanged<TWorkspace, TData>> SendEvent()
    {
        var sender = Sender();

        return new AnonymousObserver<RuleIssuesChanged<TWorkspace, TData>>(
            v => sender.OnNext(v.ToEvent()),
            exception => sender.OnError(exception),
            sender.OnCompleted);
    }
}
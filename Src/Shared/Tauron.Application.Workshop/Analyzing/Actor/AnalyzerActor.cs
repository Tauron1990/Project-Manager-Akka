using System;
using Akka.Actor;
using Tauron.Application.Workshop.Mutation;

namespace Tauron.Application.Workshop.Analyzing.Actor
{
    public sealed class AnalyzerActor<TWorkspace, TData> : ReceiveActor
        where TWorkspace : WorkspaceBase<TData> where TData : class
    {
        private readonly Lazy<IObserver<RuleIssuesChanged<TWorkspace, TData>>> _issesAction;
        private readonly TWorkspace _workspace;

        public AnalyzerActor(TWorkspace workspace, Func<IObserver<RuleIssuesChanged<TWorkspace, TData>>> issesAction)
        {
            _workspace = workspace;
            _issesAction = new Lazy<IObserver<RuleIssuesChanged<TWorkspace, TData>>>(issesAction);

            Receive<RegisterRule<TWorkspace, TData>>(RegisterRule);
            Receive<RuleIssuesChanged<TWorkspace, TData>>(OnNext);

            Receive<WatchIntrest>(wi => Context.WatchWith(wi.Target, new HandlerTerminated(wi.OnRemove)));
            Receive<HandlerTerminated>(ht => ht.Remover());
            Receive<Terminated>(_ => { });
        }

        private void OnNext(RuleIssuesChanged<TWorkspace, TData> obj)
        {
            _issesAction.Value.OnNext(obj);
        }

        private void RegisterRule(RegisterRule<TWorkspace, TData> obj)
        {
            var rule = obj.Rule;
            rule.Init(Context, _workspace);
        }

        private sealed class HandlerTerminated
        {
            internal HandlerTerminated(Action remover) => Remover = remover;

            internal Action Remover { get; }
        }
    }
}
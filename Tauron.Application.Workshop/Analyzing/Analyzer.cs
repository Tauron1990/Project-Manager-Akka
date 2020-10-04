﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Akka.Actor;
using JetBrains.Annotations;
using Tauron.Application.Workshop.Analyzing.Actor;
using Tauron.Application.Workshop.Analyzing.Core;
using Tauron.Application.Workshop.Analyzing.Rules;
using Tauron.Application.Workshop.Core;
using Tauron.Application.Workshop.Mutation;

namespace Tauron.Application.Workshop.Analyzing
{
    [PublicAPI]
    public sealed class Analyzer<TWorkspace, TData> : DeferredActor, IAnalyzer<TWorkspace, TData> 
        where TWorkspace : WorkspaceBase<TData>
    {
        private readonly HashSet<string> _rules = new HashSet<string>();

        internal Analyzer(Task<IActorRef> actor, IEventSource<IssuesEvent> source)
            : base(actor) => Issues = source;

        internal Analyzer()
            : base(Task.FromResult<IActorRef>(ActorRefs.Nobody)) 
            => Issues = new AnalyzerEventSource<TWorkspace, TData>(Task.FromResult<IActorRef>(ActorRefs.Nobody));

        public void RegisterRule(IRule<TWorkspace, TData> rule)
        {
            lock (_rules)
            {
                if (!_rules.Add(rule.Name))
                    return;
            }

            TellToActor(new RegisterRule<TWorkspace, TData>(rule));
        }

        public IEventSource<IssuesEvent> Issues { get; }
    }

    [PublicAPI]
    public static class Analyzer
    {
        public static IAnalyzer<TWorkspace, TData> From<TWorkspace, TData>(TWorkspace workspace, WorkspaceSuperviser superviser)
            where TWorkspace : WorkspaceBase<TData>
        {
            var evtSource = new SourceFabricator<TWorkspace, TData>();

            var actor = superviser.Create(Props.Create(() => new AnalyzerActor<TWorkspace, TData>(workspace, evtSource.Send)), "AnalyzerActor");
            evtSource.Init(actor);

            return new Analyzer<TWorkspace, TData>(actor, evtSource.EventSource ?? throw new InvalidOperationException("Create Analyzer"));
        }

        private class SourceFabricator<TWorkspace, TData> where TWorkspace : WorkspaceBase<TData>
        {
            public AnalyzerEventSource<TWorkspace, TData>? EventSource { get; private set; }


            public void Init(Task<IActorRef> actor) 
                => EventSource = new AnalyzerEventSource<TWorkspace, TData>(actor);

            public void Send(RuleIssuesChanged<TWorkspace, TData> evt) 
                => EventSource?.SendEvent(evt);
        }
    }
}
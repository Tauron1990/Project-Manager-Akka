using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Tauron.Application.Workshop.Analyzing.Core;
using Tauron.Application.Workshop.Analyzing.Rules;
using Tauron.Application.Workshop.Driver;
using Tauron.Application.Workshop.Mutation;

namespace Tauron.Application.Workshop.Analyzing;

[PublicAPI]
public sealed class Analyzer<TWorkspace, TData> : IAnalyzer<TWorkspace, TData>
    where TWorkspace : WorkspaceBase<TData> where TData : class
{
    private readonly Action<RegisterRule<TWorkspace, TData>> _registrar;
    private readonly HashSet<string> _rules = new();

    internal Analyzer(Action<RegisterRule<TWorkspace, TData>> registrar, IEventSource<IssuesEvent> source)
    {
        _registrar = registrar;
        Issues = source;
    }

    internal Analyzer()
    {
        Issues = new AnalyzerEventSource<TWorkspace, TData>();
        _registrar = _ => { };
    }

    public void RegisterRule(IRule<TWorkspace, TData> rule)
    {
        lock (_rules)
        {
            if (!_rules.Add(rule.Name))
                return;
        }

        _registrar(new RegisterRule<TWorkspace, TData>(rule));
    }

    public IEventSource<IssuesEvent> Issues { get; }
}

[PublicAPI]
public static class Analyzer
{
    public static IAnalyzer<TWorkspace, TData> From<TWorkspace, TData>(
        TWorkspace workspace,
        IDriverFactory factory)
        where TWorkspace : WorkspaceBase<TData> where TData : class
    {
        var evtSource = new SourceFabricator<TWorkspace, TData>();

       return new Analyzer<TWorkspace, TData>(
            factory.CreateAnalyser(workspace, evtSource.Send()),
            evtSource.EventSource ?? throw new InvalidOperationException("Create Analyzer"));
    }

    private class SourceFabricator<TWorkspace, TData> where TWorkspace : WorkspaceBase<TData> where TData : class
    {
        internal AnalyzerEventSource<TWorkspace, TData>? EventSource { get; } = new();

        internal IObserver<RuleIssuesChanged<TWorkspace, TData>> Send()
        {
            if (EventSource is null)
                throw new InvalidOperationException("Analyzer is not Initialized");

            return EventSource.SendEvent();
        }
    }
}
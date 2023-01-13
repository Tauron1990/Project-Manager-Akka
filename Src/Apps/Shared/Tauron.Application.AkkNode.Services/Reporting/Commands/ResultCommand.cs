namespace Tauron.Application.AkkaNode.Services.Reporting.Commands;

public abstract record ResultCommand<TSender, TThis, TResult> : ReporterCommandBase<TSender, TThis>
    where TSender : ISender
    where TThis : ResultCommand<TSender, TThis, TResult>;
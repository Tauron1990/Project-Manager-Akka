using JetBrains.Annotations;

namespace Tauron.Application.AkkaNode.Services.Reporting.Commands;

[PublicAPI]
public abstract record SimpleCommand<TSender, TThis> : ReporterCommandBase<TSender, TThis>
    where TThis : SimpleCommand<TSender, TThis>
    where TSender : ISender;
using System;
using System.Threading;
using System.Threading.Tasks;
using Akka.Util;
using JetBrains.Annotations;
using Tauron.Operations;
using UnitsNet;

namespace Tauron.Application.AkkaNode.Services.Reporting.Commands;

[PublicAPI]
public abstract record SimpleCommand<TSender, TThis> : ReporterCommandBase<TSender, TThis>
    where TThis : SimpleCommand<TSender, TThis>
    where TSender : ISender;

public static class SimpleCommandExtensions
{
    public static async Task<Option<Error>> Send<TSender, TCommand>(
        this TSender sender, TCommand command, Duration? timeout, Action<string> messages, CancellationToken token = default)
        where TCommand : SimpleCommand<TSender, TCommand>, IReporterMessage
        where TSender : ISender
    {
        var result = await SendingHelper.Send<object, TCommand>(sender, command, messages, timeout, isEmpty: true, token);

        return result.Fold(_ => Option<Error>.None, e => e);
    }
}
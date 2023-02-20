using System;
using System.Threading;
using System.Threading.Tasks;
using Stl;
using Tauron.Operations;
using UnitsNet;

namespace Tauron.Application.AkkaNode.Services.Reporting.Commands;

public static class SimpleCommandExtensions
{
    public static async Task<Option<Error>> Send<TSender, TCommand>(
        this TSender sender, TCommand command, Duration? timeout, Action<string> messages, CancellationToken token = default)
        where TCommand : SimpleCommand<TSender, TCommand>, IReporterMessage
        where TSender : ISender
    {
        var result = await SendingHelper.Send<object, TCommand>(sender, command, messages, timeout, isEmpty: true, token).ConfigureAwait(false);

        return result.Fold(_ => Option<Error>.None, e => e);
    }
}
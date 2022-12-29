using System;
using System.Threading;
using System.Threading.Tasks;
using Akka.Util;
using JetBrains.Annotations;
using Tauron.Operations;
using UnitsNet;

namespace Tauron.Application.AkkaNode.Services.Reporting.Commands;

public static class ResultCommandExtensions
{
    [PublicAPI]
    public static Task<Either<TResult, Error>> Send<TSender, TCommand, TResult>(
        this TSender sender, TCommand command, in Duration? timeout, TResult? resultInfo, Action<string> messages, CancellationToken token = default)
        where TSender : ISender
        where TCommand : ResultCommand<TSender, TCommand, TResult>
        => SendingHelper.Send<TResult, TCommand>(sender, command, messages, timeout, false, token);
}
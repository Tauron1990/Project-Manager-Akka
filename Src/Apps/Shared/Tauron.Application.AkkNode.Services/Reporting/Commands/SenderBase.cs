﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Akka.Util;
using JetBrains.Annotations;
using Tauron.Operations;
using UnitsNet;

namespace Tauron.Application.AkkaNode.Services.Reporting.Commands;

public sealed record ApiParameter(Duration? Timeout, CancellationToken CancellationToken, Action<string> Messages)
{
    public ApiParameter(Duration? timeout)
        : this(timeout, CancellationToken.None, _ => { }) { }

    public ApiParameter(Duration? timeout, Action<string> messages)
        : this(timeout, CancellationToken.None, messages) { }

    public ApiParameter(Duration? timeout, CancellationToken token)
        : this(timeout, token, _ => { }) { }
}

[PublicAPI]
public abstract class SenderBase<TThis, TQueryType, TCommandType> : ISender
    where TThis : SenderBase<TThis, TQueryType, TCommandType>, ISender
{
    void ISender.SendCommand(IReporterMessage command)
        => SendCommandImpl(command);

    public Task<Either<TResult, Error>> Query<TQuery, TResult>(TQuery query, ApiParameter parameter)
        where TQuery : ResultCommand<TThis, TQuery, TResult>, TQueryType
        => ((TThis)this).Send(query, parameter.Timeout, default(TResult), parameter.Messages, parameter.CancellationToken);

    public Task<Either<TResult, Error>> Query<TQuery, TResult>(ApiParameter parameter)
        where TQuery : ResultCommand<TThis, TQuery, TResult>, TQueryType, new()
        => Query<TQuery, TResult>(new TQuery(), parameter);

    public Task<Either<TResult, Error>> Query<TQuery, TResult>(TQuery query, TResult witness, ApiParameter parameter)
        where TQuery : ResultCommand<TThis, TQuery, TResult>, TQueryType
        => Query<TQuery, TResult>(query, parameter);

    public Task<Option<Error>> Command<TCommand>(TCommand command, ApiParameter parameter)
        where TCommand : SimpleCommand<TThis, TCommand>, TCommandType
        => ((TThis)this).Send(command, parameter.Timeout, parameter.Messages, parameter.CancellationToken);

    public Task<Either<TResult, Error>> Command<TCommand, TResult>(TCommand command, ApiParameter parameter)
        where TCommand : ResultCommand<TThis, TCommand, TResult>, TCommandType
        => ((TThis)this).Send(command, parameter.Timeout, default(TResult), parameter.Messages, parameter.CancellationToken);

    public Task<Either<TResult, Error>> Command<TCommand, TResult>(TCommand command, TResult witness, ApiParameter parameter)
        where TCommand : ResultCommand<TThis, TCommand, TResult>, TCommandType
        => Command<TCommand, TResult>(command, parameter);

    protected abstract void SendCommandImpl(IReporterMessage command);
}
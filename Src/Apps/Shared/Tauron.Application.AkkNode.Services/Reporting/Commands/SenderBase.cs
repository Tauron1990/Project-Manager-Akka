using System;
using System.Threading;
using System.Threading.Tasks;
using Akka.Util;
using JetBrains.Annotations;
using Tauron.Operations;

namespace Tauron.Application.AkkaNode.Services.Reporting.Commands
{
    public sealed record ApiParameter(TimeSpan Timeout, CancellationToken CancellationToken, Action<string> Messages)
    {
        public ApiParameter(TimeSpan timeout)
            : this(timeout, CancellationToken.None, _ => { }) { }

        public ApiParameter(TimeSpan timeout, Action<string> messages)
            : this(timeout, CancellationToken.None, messages) { }

        public ApiParameter(TimeSpan timeout, CancellationToken token)
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
}
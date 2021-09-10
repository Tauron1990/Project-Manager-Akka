using System;
using System.Threading.Tasks;

namespace Tauron.Application.AkkaNode.Services.Reporting.Commands
{
    public abstract class SenderBase<TThis, TQueryType, TCommandType> : ISender
        where TThis : SenderBase<TThis, TQueryType, TCommandType>, ISender
    {
        public Task<TResult> Query<TQuery, TResult>(TQuery query, TimeSpan timeout, Action<string>? mesgs = null)
            where TQuery : ResultCommand<TThis, TQuery, TResult>, TQueryType
            => ((TThis)this).Send(query, timeout, default(TResult), mesgs ?? (_ => { }));

        public Task<TResult> Query<TQuery, TResult>(TimeSpan timeout, Action<string>? mesgs = null)
            where TQuery : ResultCommand<TThis, TQuery, TResult>, TQueryType, new()
            => Query<TQuery, TResult>(new TQuery(), timeout, mesgs);

        public Task Command<TCommand>(TCommand command, TimeSpan timeout, Action<string>? msgs = null)
            where TCommand : SimpleCommand<TThis, TCommand>, TCommandType
            => ((TThis)this).Send(command, timeout, msgs ?? (_ => { }));
        
        public Task<TResult> Command<TCommand, TResult>(TCommand command, TimeSpan timeout, Action<string>? mesgs = null)
            where TCommand : ResultCommand<TThis, TCommand, TResult>, TCommandType
            => ((TThis)this).Send(command, timeout, default(TResult), mesgs ?? (_ => { }));

        protected abstract void SendCommandImpl(IReporterMessage command);

        void ISender.SendCommand(IReporterMessage command)
            => SendCommandImpl(command);
    }
}
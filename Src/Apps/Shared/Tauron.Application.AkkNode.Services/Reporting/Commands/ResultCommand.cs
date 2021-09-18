using System;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Tauron.Application.AkkaNode.Services.Reporting.Commands
{
    public abstract record ResultCommand<TSender, TThis, TResult> : ReporterCommandBase<TSender, TThis>
        where TSender : ISender
        where TThis : ResultCommand<TSender, TThis, TResult>;


    public static class ResultCommandExtensions
    {
        [PublicAPI]
        public static Task<TResult> Send<TSender, TCommand, TResult>(this TSender sender, TCommand command, TimeSpan timeout, TResult? resultInfo, Action<string> messages)
            where TSender : ISender
            where TCommand : ResultCommand<TSender, TCommand, TResult>
            => SendingHelper.Send<TResult, TCommand>(sender, command, messages, timeout, isEmpty: false);
    }
}
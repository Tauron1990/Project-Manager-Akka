using System;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Tauron.Application.AkkNode.Services.Commands
{
    [PublicAPI]
    public abstract class SimpleCommand<TSender, TThis> : ReporterCommandBase<TSender, TThis> 
        where TThis : SimpleCommand<TSender, TThis>
        where TSender : ISender { }

    public static class SimpleCommandExtensions
    {
        public static Task Send<TSender, TCommand>(this TSender sender, TCommand command, TimeSpan timeout, Action<string> messages)
            where TCommand : SimpleCommand<TSender, TCommand>, IReporterMessage
            where TSender : ISender
            => SendingHelper.Send<object, TCommand>(sender, command, messages, timeout, true);
    }
}
using System;
using System.Threading.Tasks;
using Akka.Actor;
using NLog;
using Tauron.Akka;
using Tauron.Host;

namespace Tauron.Application.AkkaNode.Services.Reporting.Commands
{
    public static class SendingHelper
    {
        private static readonly ILogger _log = LogManager.GetCurrentClassLogger();

        public static Task<TResult> Send<TResult, TCommand>(ISender sender, TCommand command, Action<string> messages, TimeSpan timeout, bool isEmpty)
            where TCommand : class, IReporterMessage
        {
            _log.Info("Sending Command {CommandType} -- {SenderType}", command.GetType(), sender.GetType());
            command.ValidateApi(sender.GetType());

            var task = new TaskCompletionSource<TResult>();
            IActorRefFactory factory;

            try
            {
                factory = ObservableActor.ExposedContext;
            }
            catch (NotSupportedException)
            {
                factory = ActorApplication.Application.ActorSystem;
            }

            var listner = Reporter.CreateListner(factory, messages, result =>
            {
                if (result.Ok)
                {
                    if (isEmpty)
                        task.SetResult(default!);
                    else if (result.Outcome is TResult outcome)
                        task.SetResult(outcome);
                    else
                        task.SetException(new InvalidCastException(result.Outcome?.GetType().Name ?? "null-source"));
                }
                else
                {
                    task.SetException(new CommandFailedException(result.Error ?? "Unkowen"));
                }
            }, timeout, null);
            command.SetListner(listner);

            sender.SendCommand(command);

            return task.Task;
        }
    }
}
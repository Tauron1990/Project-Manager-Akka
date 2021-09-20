using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Util;
using Microsoft.Extensions.Logging;
using Stl.Async;
using Tauron.AkkaHost;
using Tauron.Operations;

namespace Tauron.Application.AkkaNode.Services.Reporting.Commands
{
    public static class SendingHelper
    {
        private static readonly ILogger Log = ActorApplication.GetLogger(typeof(SendingHelper));

        public static Task<Either<TResult, Error>> Send<TResult, TCommand>(ISender sender, TCommand command, Action<string> messages, TimeSpan timeout, bool isEmpty, CancellationToken token)
            where TCommand : class, IReporterMessage
        {
            Log.LogInformation("Sending Command {CommandType} -- {SenderType}", command.GetType(), sender.GetType());
            command.ValidateApi(sender.GetType());

            var task = new TaskCompletionSource<Either<TResult, Error>>();
            IActorRefFactory factory = ActorApplication.ActorSystem;

            var listner = Reporter.CreateListner(
                factory,
                messages,
                result =>
                {
                    if (result.Ok)
                    {
                        if (isEmpty)
                            task.SetResult(Either.Left<TResult>(default!));
                        else if (result.Outcome is TResult outcome)
                            task.SetResult(Either.Left(outcome));
                        else
                            task.SetResult(Either.Right(new Error(new InvalidCastException(result.Outcome?.GetType().Name ?? "null-source"))));
                    }
                    else
                        task.SetResult(Either.Right(result.Errors?.FirstOrDefault() ?? new Error("Unkowen", "Unkowen")));
                },
                timeout);
            
            command.SetListner(listner);
            sender.SendCommand(command);

            return task.Task.WithFakeCancellation(token);
        }
    }
}
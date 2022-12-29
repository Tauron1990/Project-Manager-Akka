using System;
using System.Threading;
using System.Threading.Tasks;
using Akka.Actor;

namespace Tauron.Application.AkkaNode.Services.Core;

public sealed record QueryIsAlive
{
    public static Task<IsAliveResponse> Ask(ActorSystem system, IActorRef actor, TimeSpan timeout)
    {
        var source = new TaskCompletionSource<IsAliveResponse>();
        var cancellationTokenSource = new CancellationTokenSource(timeout);
        cancellationTokenSource.Token.Register(() => source.TrySetResult(new IsAliveResponse(false)));

        system.ActorOf(Props.Create(() => new TempActor(source, cancellationTokenSource, actor)));

        return source.Task;
    }

    private sealed class TempActor : ActorBase, IDisposable
    {
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly TaskCompletionSource<IsAliveResponse> _source;

        #pragma warning disable GU0073
        public TempActor(TaskCompletionSource<IsAliveResponse> source, CancellationTokenSource cancellation, ICanTell target)
            #pragma warning restore GU0073
        {
            _cancellationTokenSource = cancellation;
            _source = source;
            IActorContext? context = Context;
            source.Task.ContinueWith(_ => context.Stop(context.Self));


            target.Tell(new QueryIsAlive(), Self);
        }

        public void Dispose() => _cancellationTokenSource.Dispose();

        protected override bool Receive(object message)
        {
            _source.TrySetResult(message as IsAliveResponse ?? new IsAliveResponse(false));

            return true;
        }
    }
}
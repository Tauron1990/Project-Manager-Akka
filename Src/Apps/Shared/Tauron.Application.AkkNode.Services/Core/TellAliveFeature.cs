using System;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Akka.Actor;
using Tauron.Features;

namespace Tauron.Application.AkkaNode.Services.Core
{
    public interface IQueryIsAliveSupport
    {
        Task<IsAliveResponse> QueryIsAlive(ActorSystem system, TimeSpan timeout);
    }

    public sealed record QueryIsAlive
    {
        public static Task<IsAliveResponse> Ask(ActorSystem system, IActorRef actor, TimeSpan timeout)
        {
            var source = new TaskCompletionSource<IsAliveResponse>();
            var cancellationTokenSource = new CancellationTokenSource(timeout);
            cancellationTokenSource.Token.Register(() => source.TrySetResult(new IsAliveResponse(IsAlive: false)));

            system.ActorOf(Props.Create(() => new TempActor(source, cancellationTokenSource, actor)));

            return source.Task;
        }

        private sealed class TempActor : ActorBase, IDisposable
        {
            private readonly CancellationTokenSource _cancellationTokenSource;
            private readonly TaskCompletionSource<IsAliveResponse> _source;

            internal TempActor(TaskCompletionSource<IsAliveResponse> source, CancellationTokenSource cancellation, ICanTell target)
            {
                _cancellationTokenSource = cancellation;
                _source = source;
                var context = Context;
                source.Task.ContinueWith(_ => context.Stop(context.Self));


                target.Tell(new QueryIsAlive(), Self);
            }

            public void Dispose() => _cancellationTokenSource.Dispose();

            protected override bool Receive(object message)
            {
                _source.TrySetResult(message is not IsAliveResponse response ? new IsAliveResponse(IsAlive: false) : response);

                return true;
            }
        }
    }

    public sealed record IsAliveResponse(bool IsAlive);

    public sealed class TellAliveFeature : ActorFeatureBase<EmptyState>
    {
        public static IPreparedFeature New()
            => Feature.Create(() => new TellAliveFeature());

        protected override void ConfigImpl()
        {
            Receive<QueryIsAlive>(
                obs => (from ob in obs
                        from ident in ob.Sender.Ask<ActorIdentity>(new Identify(null), TimeSpan.FromSeconds(10))
                        select (ob.Sender, Response: new IsAliveResponse(IsAlive: true)))
                   .AutoSubscribe(d => d.Sender.Tell(d.Response)));
        }
    }
}
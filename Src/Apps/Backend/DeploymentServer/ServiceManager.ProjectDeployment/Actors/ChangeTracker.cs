using System;
using Akka;
using Akka.Actor;
using Akka.Streams;
using Akka.Streams.Dsl;
using Tauron.Application.AkkaNode.Services;
using Tauron.Application.Master.Commands.Deployment.Build.Data;
using Tauron.Application.Master.Commands.Deployment.Build.Querys;
using Tauron.Features;
using Tauron.Operations;

namespace ServiceManager.ProjectDeployment.Actors
{
    public sealed class ChangeTrackerActor : ReportingActor<ChangeTrackerActor.ChangeTrackeState>
    {
        public static IPreparedFeature New()
            => Feature.Create(() => new ChangeTrackerActor(),
                c =>
                {
                    var mat = c.Materializer();

                    var (queue, source) = Source.Queue<AppInfo>(10, OverflowStrategy.DropHead).PreMaterialize(mat);

                    var hub = source.ToMaterialized(BroadcastHub.Sink<AppInfo>(), Keep.Right);

                    return new ChangeTrackeState(mat, queue, hub);
                });

        private readonly ISourceQueueWithComplete<AppInfo> _appInfos;

        public ChangeTrackerActor()
        {



            Receive<QueryChangeSource>("QueryChanedSource",
                (changeSource, reporter)
                    => reporter.Compled(OperationResult.Success(new AppChangedSource(hub.Run(mat)))));
            Receive<AppInfo>(ai => _appInfos.OfferAsync(ai).PipeTo(Self));
            Receive<IQueueOfferResult>(r =>
            {
                switch (r)
                {
                    case QueueOfferResult.Failure f:
                        Log.Error(f.Cause, "Error In Change Tracker");
                        break;
                    case QueueOfferResult.QueueClosed _:
                        Log.Warning("Unexpectem Tracker Queue Close.");
                        break;
                }
            });
        }

        protected override void PostStop()
        {
            _appInfos.Complete();
            base.PostStop();
        }

        public sealed record ChangeTrackeState(ActorMaterializer Materializer, ISourceQueueWithComplete<AppInfo> AppInfos, IRunnableGraph<Source<AppInfo, NotUsed>> Hub);

        protected override void ConfigImpl()
        {
            Stop.Subscribe(_ => CurrentState.AppInfos.Complete());
        }
    }
}
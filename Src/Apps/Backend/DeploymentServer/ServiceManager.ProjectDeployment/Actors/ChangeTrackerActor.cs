using System;
using System.Reactive.Linq;
using Akka;
using Akka.Actor;
using Akka.Streams;
using Akka.Streams.Dsl;
using Akka.Streams.Implementation.Fusing;
using Microsoft.Extensions.Logging;
using Tauron;
using Tauron.Application;
using Tauron.Application.AkkaNode.Services.Reporting;
using Tauron.Application.Master.Commands.Deployment.Build.Data;
using Tauron.Application.Master.Commands.Deployment.Build.Querys;
using Tauron.Features;
using Tauron.Operations;

namespace ServiceManager.ProjectDeployment.Actors
{
    public sealed class ChangeTrackerActor : ReportingActor<ChangeTrackerActor.ChangeTrackeState>
    {
        public static IPreparedFeature New()
            => Feature.Create(
                () => new ChangeTrackerActor(),
                c =>
                {
                    var mat = c.Materializer();

                    var (queue, source) = Source.Queue<AppInfo>(10, OverflowStrategy.DropHead).PreMaterialize(mat);

                    var hub = source.ToMaterialized(BroadcastHub.Sink<AppInfo>(), Keep.Right);

                    return new ChangeTrackeState(mat, queue, hub);
                });

        protected override void ConfigImpl()
        {
            var logger = TauronEnviroment.LoggerFactory.CreateLogger<ChangeTrackerActor>();
            
            Stop.Subscribe(_ => CurrentState.AppInfos.Complete());
            TryReceive<QueryAppChangeSource>(
                "QueryChanedSource",
                obs => obs.ToUnit(m => m.Reporter.Compled(OperationResult.Success(new AppChangedSource(m.State.Hub.Run(m.State.Materializer))))));

            Receive<AppInfo>(obs => obs.ToUnit(m => m.State.AppInfos.OfferAsync(m.Event).PipeTo(Self)));

            Receive<IQueueOfferResult>(
                obs => obs
                   .Select(m => m.Event)
                   .SubscribeWithStatus(
                        p =>
                        {
                            switch (p)
                            {
                                case QueueOfferResult.Failure f:
                                    logger.LogError(f.Cause, "Error In Change Tracker");

                                    break;
                                case QueueOfferResult.QueueClosed _:
                                    logger.LogError("Unexpectem Tracker Queue Close");

                                    break;
                            }
                        }));
        }

        public sealed record ChangeTrackeState(ActorMaterializer Materializer, ISourceQueueWithComplete<AppInfo> AppInfos, IRunnableGraph<Source<AppInfo, NotUsed>> Hub);
    }
}
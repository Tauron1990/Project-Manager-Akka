using System;
using System.Reactive;
using System.Reactive.Linq;
using Akka.Actor;
using JetBrains.Annotations;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using Tauron.Akka;
using Tauron.Application.AkkNode.Services.Features;

namespace Tauron.Application.AkkNode.Services.CleanUp
{
    [PublicAPI]
    public sealed class CleanUpManager : FeatureActorBase<CleanUpManager, CleanUpManager.CleanUpManagerState>
    {
        public static readonly InitCleanUp Initialization = new();

        public Props Create(IMongoDatabase database, string cleanUpCollection, IMongoCollection<ToDeleteRevision> revisions, GridFSBucket bucked)
            => Create(new CleanUpManagerState(database, cleanUpCollection, revisions, bucked),
                      () => new[] { Make.Feature(Initializing) });

        private static void Initializing(IFeatureActor<CleanUpManagerState> actorBase)
            => actorBase.WhenReceive<InitCleanUp>(obs => obs.Select(init =>
                                                                    {
                                                                        var (database, cleanUpCollection, _, _) = init.State;

                                                                        if (!database.ListCollectionNames().Contains(s => s == cleanUpCollection))
                                                                            database.CreateCollection("CleanUp", new CreateCollectionOptions {Capped = true, MaxDocuments = 1, MaxSize = 1024});
                                                                        return init;
                                                                    })
                                                            .Select(init => init.State.Database.GetCollection<CleanUpTime>(init.State.CleanUpCollection))
                                                            .SelectMany(async db => new {Data = await db.AsQueryable().FirstOrDefaultAsync(), Datbase = db})
                                                            .SelectMany(async db =>
                                                                        {
                                                                            if (db.Data == null)
                                                                                await db.Datbase.InsertOneAsync(new CleanUpTime(default, TimeSpan.FromDays(7), DateTime.Now));

                                                                            return Unit.Default;
                                                                        })
                                                            .ObserveOnSelf()
                                                            .SubscribeWithStatus(_ =>
                                                                                 {
                                                                                     actorBase.Timers.StartPeriodicTimer(Initialization, new StartCleanUp(), TimeSpan.FromHours(1));
                                                                                     actorBase.Become(new[] {Simple.Feature(Running)});
                                                                                 }));

        private static void Running(IFeatureActor<CleanUpManagerState> actorBase)
            => actorBase.WhenReceive<StartCleanUp>(obs => obs.Select(data => CleanUpOperator.Create(data.State.Database.GetCollection<CleanUpTime>(data.State.CleanUpCollection),
                                                                                                                    data.State.Revisions, data.State.Bucket))
                                                             .ForwardToActor(props => actorBase.Context.ActorOf(props)));

        public sealed record InitCleanUp
        {
            internal InitCleanUp()
            {

            }
        }

        public sealed record CleanUpManagerState(IMongoDatabase Database, string CleanUpCollection, IMongoCollection<ToDeleteRevision> Revisions, GridFSBucket Bucket);
    }
}
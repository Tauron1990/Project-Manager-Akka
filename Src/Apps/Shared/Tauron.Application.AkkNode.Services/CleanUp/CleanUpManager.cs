using System;
using System.Reactive.Linq;
using JetBrains.Annotations;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using SharpRepository.Repository;
using SharpRepository.Repository.Configuration;
using Tauron.Akka;
using Tauron.Features;
using YellowDrawer.Storage.Common;

namespace Tauron.Application.AkkaNode.Services.CleanUp
{
    [PublicAPI]
    public sealed class CleanUpManager : ActorFeatureBase<CleanUpManager.CleanUpManagerState>
    {
        public static readonly InitCleanUp Initialization = new();

        [PublicAPI]
        public static IPreparedFeature New(ISharpRepositoryConfiguration repositoryConfiguration, IStorageProvider storageProvider)
            => Feature.Create(() => new CleanUpManager(),
                _ => new CleanUpManagerState(database, cleanUpCollection, revisions, bucked, false));

        protected override void ConfigImpl()
        {
            Receive<InitCleanUp>(obs => obs.Where(m => !m.State.IsRunning)
                .Select(init =>
                {
                    var (database, cleanUpCollection, _, _, _) = init.State;

                    if (!database.ListCollectionNames().Contains(s => s == cleanUpCollection))
                        database.CreateCollection("CleanUp",
                            new CreateCollectionOptions {Capped = true, MaxDocuments = 1, MaxSize = 1024});
                    return init;
                })
                .Select(init => new
                {
                    Database = init.State.Database.GetCollection<CleanUpTime>(init.State.CleanUpCollection), init.Timers
                })
                .SelectMany(async data => new
                    {Data = await data.Database.AsQueryable().FirstOrDefaultAsync(), data.Database, data.Timers})
                .SelectMany(async data =>
                {
                    if (data.Data == null)
                        await data.Database.InsertOneAsync(new CleanUpTime(default, TimeSpan.FromDays(7),
                            DateTime.Now));

                    return data.Timers;
                })
                .ObserveOnSelf()
                .ToUnit(d => d.StartPeriodicTimer(Initialization, new StartCleanUp(), TimeSpan.FromHours(1))));

            Receive<StartCleanUp>(obs => obs.Where(m => m.State.IsRunning)
                .Select(data => CleanUpOperator.New(
                    data.State.Database.GetCollection<CleanUpTime>(data.State.CleanUpCollection),
                    data.State.Revisions, data.State.Bucket))
                .ForwardToActor(props => Context.ActorOf(props)));
        }

        public sealed record CleanUpManagerState(ISharpRepositoryConfiguration Database, IRepository<CleanUpTime, string> CleabUpRepository, IRepository<ToDeleteRevision, string> Revisions, 
            IStorageProvider Bucket, bool IsRunning);

        public sealed record InitCleanUp
        {
            internal InitCleanUp()
            {
            }
        }
    }
}
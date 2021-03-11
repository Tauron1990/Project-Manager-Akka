using System;
using System.Linq;
using System.Reactive.Linq;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using Tauron.Application.AkkaNode.Services.CleanUp;
using Tauron.Application.AkkaNode.Services.CleanUp.Core;
using Tauron.Features;

namespace Tauron.Application.AkkaNode.Services.CleanOld
{
    public sealed class CleanUpOperator : ActorFeatureBase<CleanUpOperator.State>
    {
        public static IPreparedFeature New(IMongoCollection<CleanUpTime> cleanUp,
            IMongoCollection<ToDeleteRevision> revisions, GridFSBucket bucket)
            => Feature.Create(() => new CleanUpOperator(), _ => new State(cleanUp, revisions, bucket));

        protected override void Config()
            => Receive<StartCleanUp>(obs => obs.Take(1)
                .SelectMany(async s => new {s.State, Data = await s.State.CleanUp.AsQueryable().FirstAsync()})
                .Where(d => d.Data.Last + d.Data.Interval < DateTime.Now)
                .SelectMany(d => d.State.Revisions
                    .AsQueryable().ToCursor().ToEnumerable()
                    .Select(revision =>
                    {
                        var (_, buckedId) = revision;
                        d.State.Bucked.Delete(ObjectId.Parse(buckedId));
                        return Builders<ToDeleteRevision>.Filter.Eq(r => r.BuckedId == buckedId, true);
                    }))
                .Finally(() => Context.Stop(Self))
                .Subscribe(_ => { }, ex => Log.Error(ex, "Error on Clean up Database")));

        public sealed record State(IMongoCollection<CleanUpTime> CleanUp, IMongoCollection<ToDeleteRevision> Revisions,
            GridFSBucket Bucked);
    }
}
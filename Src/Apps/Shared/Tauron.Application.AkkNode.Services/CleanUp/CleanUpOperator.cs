using System;
using System.Linq;
using System.Reactive.Linq;
using Akka.Actor;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using Tauron.Application.AkkNode.Services.Features;

namespace Tauron.Application.AkkNode.Services.CleanUp
{


    public sealed class CleanUpOperator : FeatureActorBase<CleanUpOperator, CleanUpOperator.State>
    {
        public static Props Create(IMongoCollection<CleanUpTime> cleanUp, IMongoCollection<ToDeleteRevision> revisions, GridFSBucket bucket) 
            => Create(new State(cleanUp, revisions, bucket), Make.Feature(CleanUp));

        private static void CleanUp(IFeatureActor<State> actor)
            => actor.WhenReceive<StartCleanUp>(obs => obs.Take(1)
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
                                                         .Finally(() => actor.Context.Stop(actor.Self))
                                                         .Subscribe(_ => { }, ex => actor.Log.Error(ex, "Error on Clean up Database")));

        public sealed record State(IMongoCollection<CleanUpTime> CleanUp, IMongoCollection<ToDeleteRevision> Revisions, GridFSBucket Bucked);
    }
}
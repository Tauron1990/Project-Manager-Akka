using System;
using System.Reactive.Linq;
using MongoDB.Bson;
using Tauron.Application.AkkaNode.Services.MongoDb;
using Tauron.Application.Workshop.Mutating;
using Tauron.Application.Workshop.StateManagement.Attributes;
using RevisionList = Tauron.Application.Workshop.Mutating.MutatingContext<System.Collections.Immutable.ImmutableList<Tauron.Application.AkkaNode.Services.CleanUp.Core.ToDeleteRevision>>

namespace Tauron.Application.AkkaNode.Services.CleanUp.Core
{
    [BelogsToState(typeof(CleanUpManager))]
    public static class CleanUpReducer
    {
        [Reducer]
        public static IObservable<MutatingContext<CleanUpTime?>> InitCleanUp(IObservable<MutatingContext<CleanUpTime?>> input, InitializeCleanUpAction action)
        {
            return input.Select(c => c.Data == null
                                    ? c.WithChange(new InitCompled())
                                    : c.Update(new InitCompled(), new CleanUpTime(ObjectId.Empty, TimeSpan.FromDays(7), DateTime.Now, true)));
        }

        [Reducer]
        public static IObservable<MutatingContext<CleanUpTime>> StartCleanUp(IObservable<MutatingContext<CleanUpTime>> input, StartCleanUp acion) 
            => input.Select(d => d.WithChange(new StartCleanUpEvent(d.Data.Last + d.Data.Interval < DateTime.Now)));

        [Reducer]
        public static IObservable<RevisionList> DeletData(IObservable<RevisionList> input, RunCleanUpAction action)
        {

        }
    }
}
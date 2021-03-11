using System;
using System.Reactive.Linq;
using MongoDB.Bson;
using Tauron.Application.Workshop.Mutating;
using Tauron.Application.Workshop.StateManagement.Attributes;

namespace Tauron.Application.AkkaNode.Services.CleanUp.Core
{
    [BelogsToState(typeof(CleanUpManager))]
    public static class CleanUpReducer
    {


        [Reducer]
        public static IObservable<MutatingContext<CleanUpTime?>> InitCleanUp(IObservable<MutatingContext<CleanUpTime?>> input, InitializeCleanUpCommand command)
        {
            return input.Select(c => c.Data == null 
                                    ? c.WithChange(new InitCompled()) 
                                    : c.Update(new InitCompled(), new CleanUpTime(ObjectId.Empty, TimeSpan.FromDays(7), DateTime.Now, true)));
        }
    }
}
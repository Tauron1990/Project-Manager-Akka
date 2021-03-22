using System;
using System.Collections.Immutable;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using MongoDB.Driver.GridFS;
using Tauron.Application.AkkaNode.Services.CleanUp.Core;
using Tauron.Application.AkkaNode.Services.MongoDb;
using Tauron.Application.Workshop.Mutating;
using Tauron.Application.Workshop.Mutation;
using Tauron.Application.Workshop.StateManagement;
using Tauron.Application.Workshop.StateManagement.Attributes;
using Tauron.Application.Workshop.StateManagement.StatePooling;
using Tauron.Features;

namespace Tauron.Application.AkkaNode.Services.CleanUp
{
    [State(typeof(CleanUpTime), typeof(GridFSBucketEntity), typeof(ImmutableList<ToDeleteRevision>))]
    [DefaultDispatcher]
    public sealed class CleanUpManager : ActorFeatureStateBase<EmptyState>, IPooledState, IPostInit, IInitState<CleanUpTime>, IGetSource<GridFSBucketEntity>
    {
        private static readonly object Key = new();

        private IActionInvoker _actionInvoker = RootManager.Empty;
        private IObservable<GridFSBucket> _bucked;

        protected override void ConfigImpl()
        {
            Receive<StartCleanUp>(obs => obs
                                        .Select(p => p.Event)
                                        .ToActionInvoker(_actionInvoker));
        }

        public void Init(IActionInvoker invoker)
        {
            _actionInvoker = invoker;
            invoker.Run(new InitializeCleanUpAction());
        }

        public void Init(ExtendedMutatingEngine<MutatingContext<CleanUpTime>> engine)
        {
            engine.EventSource<CleanUpTime, InitCompled>()
                  .Select(_ => Timers)
                  .SubscribeWithStatus(t => t.StartPeriodicTimer(Key, new StartCleanUp(), TimeSpan.FromHours(1)));


            engine.EventSource<CleanUpTime, StartCleanUpEvent>()
                  .Where(evt => evt.Run)
                  .Select(_ => new RunCleanUpAction(_bucked));

        }

        public void DataSource(IExtendedDataSource<MutatingContext<GridFSBucketEntity>> dataSource) 
            => _bucked = dataSource.GetData(EmptyQuery.Instance).ToObservable().Select(e => e.Data.Bucket);
    }
}
using System;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading;
using System.Threading.Tasks;
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
    [State]
    [DefaultDispatcher]
    public sealed class CleanUpManager : ActorFeatureStateBase<EmptyState>, IState<ToDeleteRevision>, IState<CleanUpTime>, IState<GridFSBucketEntity>,
        IPooledState, IPostInit, IInitState<CleanUpTime>, IGetSource<GridFSBucketEntity>
    {
        private static readonly object Key = new();

        private IActionInvoker _actionInvoker = RootManager.Empty;
        private IObservable<GridFSBucketEntity> _bucked = Observable.Empty<GridFSBucketEntity>();

        protected override void ConfigImpl()
        {
            Receive<StartCleanUp>(obs => obs
                                        .Select(p => p.Event)
                                        .ToActionInvoker(_actionInvoker));
        }

        public void Init(IActionInvoker invoker)
        {
            _actionInvoker = invoker;
            invoker.Run(new InitializeCleanUpCommand());
        }

        public void Init(ExtendedMutatingEngine<MutatingContext<CleanUpTime>> engine)
        {
            engine.EventSource<CleanUpTime, InitCompled>()
                  .Select(_ => Timers)
                  .SubscribeWithStatus(t => t.StartPeriodicTimer(Key, new StartCleanUp(), TimeSpan.FromHours(1)));
        }

        public void DataSource(IExtendedDataSource<MutatingContext<GridFSBucketEntity>> dataSource) 
            => _bucked = dataSource.GetData(EmptyQuery.Instance).ToObservable().Select(b => b.Data);
    }
}
using System;
using System.Collections.Immutable;
using System.Reactive.Linq;
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
    public sealed class CleanUpManager : ActorFeatureStateBase<EmptyState>, IPooledState, IPostInit, IInitState<CleanUpTime>, IInitState<GridFSBucketEntity>
    {
        private static readonly object Key = new();

        private IActionInvoker _actionInvoker = RootManager.Empty;

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
        }

        public void Init(ExtendedMutatingEngine<MutatingContext<GridFSBucketEntity>> engine)
        {
            engine.EventSource<GridFSBucketEntity, StartCleanUpEvent>()
                  .Where(sc => sc.Action != null)
                  .Select(sc => sc.Action)
                  .ToActionInvoker(_actionInvoker);
        }
    }
}
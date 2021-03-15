using System.Reactive.Linq;
using System.Threading.Tasks;
using Akka.Actor;
using JetBrains.Annotations;
using Tauron.Application.Workshop.Mutating;
using Tauron.Application.Workshop.Mutation;
using Tauron.Application.Workshop.StateManagement.Internal;
using Tauron.Application.Workshop.StateManagement.StatePooling;
using Tauron.Features;

namespace Tauron.Application.Workshop.StateManagement
{
    [PublicAPI]
    public abstract class StateBase<TData> : IState, ICanQuery<TData>
        where TData : class, IStateEntity
    {
        private IExtendedDataSource<MutatingContext<TData>>? _source;

        protected StateBase(ExtendedMutatingEngine<MutatingContext<TData>> engine)
        {
            OnChange = engine.EventSource(c => c.Data);
        }

        public IEventSource<TData> OnChange { get; }

        void IGetSource<TData>.DataSource(IExtendedDataSource<MutatingContext<TData>> source)
        {
            _source = source;
        }

        public async Task<TData?> Query(IQuery query)
        {
            var source = _source;
            try
            {
                return source == null ? null : (await source.GetData(query)).Data;
            }
            finally
            {
                if (source != null)
                    await source.OnCompled(query);
            }
        }
    }

    [PublicAPI]
    public abstract class ActorStateBase : ReceiveActor
    {
        protected override bool AroundReceive(Receive receive, object message)
        {
            if (message is StateActorMessage msg)
            {
                msg.Apply(this);
                return true;
            }

            return base.AroundReceive(receive, message);
        }
    }

    [PublicAPI]
    public abstract class ActorFeatureStateBase<TState> : ActorFeatureBase<TState>
    {
        protected override void Config()
        {
            Receive<StateActorMessage>(obs => obs.Select(m => m.Event).SubscribeWithStatus(m => m.Apply(this)));

            base.Config();
        }
    }
}
using Tauron.Application.Workshop.Mutating;
using Tauron.Application.Workshop.Mutation;
using Tauron.Application.Workshop.StateManagement.StatePooling;

namespace Tauron.Application.Workshop.StateManagement.Akka.Internal;

public abstract class StateActorMessage
{
    public static StateActorMessage Create<TData>(IExtendedDataSource<MutatingContext<TData>> source)
        where TData : class, IStateEntity => new QueryMessage<TData>(source);

    public static StateActorMessage Create<TData>(ExtendedMutatingEngine<MutatingContext<TData>> engine)
        => new InitMessage<TData>(engine);

    public static StateActorMessage Create(IActionInvoker actionInvoker)
        => new PostInit(actionInvoker);

    public abstract void Apply(object @this);

    private sealed class InitMessage<TData> : StateActorMessage
    {
        private readonly ExtendedMutatingEngine<MutatingContext<TData>> _engine;

        internal InitMessage(ExtendedMutatingEngine<MutatingContext<TData>> engine) => _engine = engine;

        public override void Apply(object @this)
        {
            if(@this is IInitState<TData> state)
                state.Init(_engine);
        }
    }

    private sealed class QueryMessage<TData> : StateActorMessage
        where TData : class, IStateEntity
    {
        private readonly IExtendedDataSource<MutatingContext<TData>> _dataSource;

        internal QueryMessage(IExtendedDataSource<MutatingContext<TData>> dataSource) => _dataSource = dataSource;

        public override void Apply(object @this)
        {
            if(@this is ICanQuery<TData> query)
                query.DataSource(_dataSource);
        }
    }

    private sealed class PostInit : StateActorMessage
    {
        private readonly IActionInvoker _invoker;

        internal PostInit(IActionInvoker invoker) => _invoker = invoker;

        public override void Apply(object @this)
        {
            if(@this is IPostInit postInit)
                postInit.Init(_invoker);
        }
    }
}
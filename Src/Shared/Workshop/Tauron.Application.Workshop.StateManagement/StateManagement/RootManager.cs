using System.Collections.Concurrent;
using System.Reactive;
using JetBrains.Annotations;
using Tauron.Application.Workshop.Driver;
using Tauron.Application.Workshop.Mutation;
using Tauron.Application.Workshop.StateManagement.Builder;
using Tauron.Application.Workshop.StateManagement.Dispatcher;
using Tauron.Application.Workshop.StateManagement.Internal;
using Tauron.Application.Workshop.StateManagement.StatePooling;
using Tauron.Operations;

namespace Tauron.Application.Workshop.StateManagement;

[PublicAPI]
public sealed class RootManager : DisposeableBase, IActionInvoker
{
    public static readonly IActionInvoker Empty = new EmptyInvoker();
    private readonly IDriverFactory _driverFactory;

    private readonly IEffect[] _effects;
    private readonly MutatingEngine _engine;
    private readonly IMiddleware[] _middlewares;
    private readonly bool _sendBackSetting;
    private readonly ConcurrentDictionary<string, ConcurrentBag<StateContainer>> _stateContainers = new();
    private readonly StateContainer[] _states;

    internal RootManager(
        IDriverFactory driverFactory, IStateDispatcherConfigurator stateDispatcher, IEnumerable<StateBuilderBase> states,
        IEnumerable<IEffect?> effects, IEnumerable<IMiddleware?> middlewares, bool sendBackSetting, IServiceProvider? serviceProvider,
        IStateInstanceFactory[] instanceFactories)
    {
        _driverFactory = driverFactory;
        _sendBackSetting = sendBackSetting;
        _engine = MutatingEngine.Create(stateDispatcher.Configurate(driverFactory));
        _effects = effects.Where(e => e != null).ToArray()!;
        _middlewares = middlewares.Where(m => m != null).ToArray()!;

        var builderParameter = new StateBuilderParameter(_engine, serviceProvider, this, new StatePool(), new DispatcherPool(), instanceFactories);

        foreach (StateBuilderBase stateBuilder in states)
        {
            (StateContainer container, string key) = stateBuilder.Materialize(builderParameter);
            _stateContainers.GetOrAdd(key, _ => new ConcurrentBag<StateContainer>()).Add(container);
        }

        _states = _stateContainers.SelectMany(b => b.Value).ToArray();

        foreach (IMiddleware middleware in _middlewares)
            middleware.Initialize(this);

        foreach (IMiddleware middleware in _middlewares)
            middleware.AfterInitializeAllMiddlewares();
    }

    public TState? GetState<TState>()
        where TState : class
        => GetState<TState>("");

    public TState? GetState<TState>(string key)
        where TState : class
    {
        if(!_stateContainers.TryGetValue(key, out var bag)) return null;

        foreach (StateContainer stateContainer in bag)
            if(stateContainer.Instance.ActualState is TState state)
                return state;

        return null;
    }

    public void Run(IStateAction action, bool? sendBack)
    {
        if(_middlewares.Any(m => !m.MayDispatchAction(action)))
            return;

        _middlewares.Foreach(m => m.BeforeDispatch(action));

        var sender = _driverFactory.GetResultSender();

        var effects = new EffectInvoker(_effects.Where(e => e.ShouldReactToAction(action)), action, this);
        var resultInvoker = new ResultInvoker(effects, _engine, sender, sendBack ?? _sendBackSetting, action);

        foreach (IDataMutation? dataMutation in _states.Select(
                     sc
                         => sc.TryDipatch(action, resultInvoker.AddResult(), resultInvoker.WorkCompled())))
        {
            if(dataMutation == null) continue;

            resultInvoker.PushWork();
            _engine.Mutate(dataMutation);
        }
    }

    internal void PostInit()
    {
        foreach (IStateInstance instance in _states.Select(s => s.Instance))
            instance.PostInit(this);
    }

    protected override void DisposeCore(bool disposing)
    {
        if(!disposing) return;

        _stateContainers.Values.Foreach(s => s.Foreach(d => d.Dispose()));
        _stateContainers.Clear();
    }

    private sealed class EmptyInvoker : IActionInvoker
    {
        public TState? GetState<TState>() where TState : class => null;

        public TState? GetState<TState>(string key) where TState : class => null;

        public void Run(IStateAction action, bool? sendBack = null) { }
    }

    private sealed class ResultInvoker : ISyncMutation
    {
        private readonly IStateAction _action;
        private readonly EffectInvoker _effectInvoker;
        private readonly MutatingEngine _mutatingEngine;
        private readonly ConcurrentBag<IReducerResult> _results = new();
        private readonly bool _sendBack;
        private readonly Action<IOperationResult>? _sender;
        private int _pending;

        private IObserver<IReducerResult>? _result;
        private IObserver<Unit>? _workCompled;

        internal ResultInvoker(
            EffectInvoker effectInvoker, MutatingEngine mutatingEngine, Action<IOperationResult>? sender,
            bool sendBack, IStateAction action)
        {
            _effectInvoker = effectInvoker;
            _mutatingEngine = mutatingEngine;
            _sender = sender;
            _sendBack = sendBack;
            _action = action;
        }

        public string Name => "SendBack";
        public Action Run => Runner;

        private void Runner()
        {
            if(!_sendBack || _sender is null) return;

            var errors = new List<string>();
            var fail = false;

            foreach (IReducerResult result in _results)
            {
                if(result.IsOk) continue;

                fail = true;
                errors.AddRange(result.Errors ?? Array.Empty<string>());
            }

            _sender(
                fail
                    ? OperationResult.Failure(errors.Select(s => new Error(s, s)), _action)
                    : OperationResult.Success(_action));
        }

        internal void PushWork()
            => Interlocked.Increment(ref _pending);

        internal IObserver<IReducerResult> AddResult()
            => _result ??= new AnonymousObserver<IReducerResult>(n => _results.Add(n), _ => { });

        internal IObserver<Unit> WorkCompled()
        {
            void Compled()
            {
                if(Interlocked.Decrement(ref _pending) != 0) return;

                _mutatingEngine.Mutate(_effectInvoker);
                _mutatingEngine.Mutate(this);
            }

            return _workCompled ??= new AnonymousObserver<Unit>(
                _ => { },
                e =>
                {
                    _results.Add(new ErrorResult(e));
                    Compled();
                },
                Compled);
        }
    }

    private sealed class EffectInvoker : ISyncMutation
    {
        private readonly IStateAction _action;
        private readonly IEnumerable<IEffect> _effects;
        private readonly IActionInvoker _invoker;

        internal EffectInvoker(IEnumerable<IEffect> effects, IStateAction action, IActionInvoker invoker)
        {
            _effects = effects;
            _action = action;
            _invoker = invoker;
        }

        public string Name => "Invoke Effects";
        public Action Run => () => _effects.Foreach(e => e.Handle(_action, _invoker));
    }
}
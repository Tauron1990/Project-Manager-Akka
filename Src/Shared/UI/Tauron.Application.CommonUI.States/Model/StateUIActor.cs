﻿using JetBrains.Annotations;
using Tauron.Application.CommonUI.AppCore;
using Tauron.Application.Workshop;
using Tauron.Application.Workshop.Mutation;
using Tauron.Application.Workshop.StateManagement;
using Tauron.Operations;
using Tauron.TAkka;

namespace Tauron.Application.CommonUI.Model;

[PublicAPI]
[MeansImplicitUse(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
public abstract class StateUIActor : UiActor
{
    private readonly Dictionary<Type, Action<IOperationResult>> _compledActions = new();

    protected StateUIActor(IServiceProvider serviceProvider, IUIDispatcher dispatcher, IActionInvoker actionInvoker) :
        base(serviceProvider, dispatcher)
    {
        ActionInvoker = actionInvoker;
        Receive<IOperationResult>(obs => obs.SubscribeWithStatus(InternalOnOperationCompled));
    }

    public IActionInvoker ActionInvoker { get; }

    private void InternalOnOperationCompled(IOperationResult result)
    {
        Type? actionType = result.Outcome?.GetType();

        if(actionType?.IsAssignableTo(typeof(IStateAction)) != true) return;

        if(_compledActions.TryGetValue(actionType, out var action))
        {
            action(result);

            return;
        }

        OnOperationCompled(result);
    }

    protected virtual void OnOperationCompled(IOperationResult result) { }

    public TState GetState<TState>(string key = "") where TState : class => ActionInvoker.GetState<TState>(key) ??
                                                                            throw new InvalidOperationException("No such State Found");

    public void ConfigurateState<TState>(Action<TState> toConfig)
        where TState : class
    {
        ConfigurateState(string.Empty, toConfig);
    }

    public void ConfigurateState<TState>(string key, Action<TState> toConfig) where TState : class
    {
        var state = GetState<TState>(key);
        toConfig(state);
    }

    public void WhenActionComnpled<TAction>(Action<IOperationResult> opsAction)
        where TAction : IStateAction
    {
        Type key = typeof(TAction);
        _compledActions[key] = opsAction.Combine(_compledActions.GetValueOrDefault(key))!;
    }

    public UIStateConfiguration<TState> WhenStateChanges<TState>(string? name = null)
        where TState : class
        => new(
            ActionInvoker.GetState<TState>(name ?? string.Empty) ??
            throw new ArgumentException("No such State Found", nameof(name)),
            this);

    public void DispatchAction(IStateAction action, bool? sendBack = true) => ActionInvoker.Run(action, sendBack);

    [PublicAPI]
    public sealed class UIStateConfiguration<TState>
    {
        private readonly StateUIActor _actor;
        private readonly TState _state;

        public UIStateConfiguration(TState state, StateUIActor actor)
        {
            _state = state;
            _actor = actor;
        }

        public UIStateEventConfiguration<TEvent> FromEvent<TEvent>(
            Func<TState, IEventSource<TEvent>> source,
            Action<UIStateEventConfiguration<TEvent>>? configAction = null)
        {
            var config = new UIStateEventConfiguration<TEvent>(source(_state), _actor);
            configAction?.Invoke(config);

            return config;
        }
    }

    [PublicAPI]
    public sealed class UIStateEventConfiguration<TEvent>
    {
        private readonly StateUIActor _actor;
        private readonly IEventSource<TEvent> _eventSource;

        public UIStateEventConfiguration(IEventSource<TEvent> eventSource, StateUIActor actor)
        {
            _eventSource = eventSource;
            _actor = actor;
        }

        public FluentPropertyRegistration<TData> ToProperty<TData>(
            string name, Func<TEvent, TData> transform,
            Func<TEvent, bool>? condition = null)
        {
            var propertyConfig = _actor.RegisterProperty<TData>(name);
            var property = propertyConfig.Property;

            _eventSource.RespondOn(
                _actor.Self,
                evt =>
                {
                    if(condition != null && !condition(evt))
                        return;

                    property.Set(transform(evt));
                });

            return propertyConfig;
        }

        public void ToAction(Action<TEvent> action)
        {
            _eventSource.RespondOn(_actor.Self);
            #pragma warning disable GU0011
            _actor.Receive(action);
            #pragma warning restore GU0011
        }

        public void ToObservable(Action<IObservable<TEvent>> observableAction)
        {
            observableAction(_eventSource.ObserveOnSelf());
        }
    }
}
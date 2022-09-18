using JetBrains.Annotations;

namespace Tauron.Application.Akka.Redux.Configuration;

[PublicAPI]
public interface IReducerFactory<TState>
	where TState : class, new()
{
	On<TState> On<TAction>(Func<TState, TAction, TState> reducer) where TAction : class;

	On<TState> On<TAction>(Func<TState, TState> reducer) where TAction : class;


	IStateLens<TState, TFeatureState> CreateSubReducers<TFeatureState>(Func<TState, TFeatureState> featureSelector, Func<TState, TFeatureState, TState> stateReducer)
		where TFeatureState : class, new();
}

[PublicAPI]
public interface IEffectFactory<TState>
	where TState : class, new()
{
	IEffect<TState> CreateEffect(Func<IObservable<object>> run);

	IEffect<TState> CreateEffect(Func<IObservable<TState>, IObservable<object>> run);

	IEffect<TState> CreateEffect<TAction>(Func<IObservable<(TAction Action, TState State)>, IObservable<object>> run)
		where TAction : class;
}

[PublicAPI]
public interface IRequestFactory<TState>
{
	IRequestFactory<TState> AddRequest<TAction>(Func<TAction, CancellationToken, ValueTask<string?>> runRequest, Func<TState, TAction, TState> onScess)
		where TAction : class;
	
	IRequestFactory<TState> AddRequest<TAction>(
		Func<TAction, CancellationToken, ValueTask<string?>> runRequest, 
		Func<TState, TAction, TState> onScess,
		Func<TState, object, TState> onFail)
		where TAction : class;
	
	IRequestFactory<TState> AddRequest<TAction>(Func<TAction, CancellationToken, Task<string?>> runRequest, Func<TState, TAction, TState> onScess)
		where TAction : class;
	
	IRequestFactory<TState> AddRequest<TAction>(
		Func<TAction, CancellationToken, Task<string?>> runRequest, 
		Func<TState, TAction, TState> onScess,
		Func<TState, object, TState> onFail)
		where TAction : class;

	IRequestFactory<TState> OnTheFlyUpdate<TSource, TData>(
		Func<TState, TSource> sourceSelector,
		Func<CancellationToken, Func<CancellationToken, ValueTask<TSource>>, Task<TData>> fetcher,
		Func<TState, TData, TState> patcher);
	
	IRequestFactory<TState> OnTheFlyUpdate<TData>(
		Func<CancellationToken, Task<TData>> fetcher,
		Func<TState, TData, TState> patcher);
}

[PublicAPI]
public interface IStateConfiguration<TActualState>
	where TActualState : class, new()
{
    IStateConfiguration<TActualState> ApplyReducer(Func<IReducerFactory<TActualState>, On<TActualState>> factory);

    IStateConfiguration<TActualState> ApplyReducers(params Func<IReducerFactory<TActualState>, On<TActualState>>[] factorys);

    IStateConfiguration<TActualState> ApplyReducers(Func<IReducerFactory<TActualState>, IEnumerable<On<TActualState>>> factory);

    IStateConfiguration<TActualState> ApplyEffect(Func<IEffectFactory<TActualState>, IEffect<TActualState>> factory);

    IStateConfiguration<TActualState> ApplyEffects(params Func<IEffectFactory<TActualState>, IEffect<TActualState>>[] factorys);

    IStateConfiguration<TActualState> ApplyEffects(Func<IEffectFactory<TActualState>, IEnumerable<IEffect<TActualState>>> factory);

    IStateConfiguration<TActualState> ApplyRequests(Action<IRequestFactory<TActualState>> factory);

    IStateConfiguration<TActualState> ApplyRequests(object factory, params Action<IRequestFactory<TActualState>>[] factorys);

    IConfiguredState AndFinish(Action<IRootStore>? onCreate = null);
}
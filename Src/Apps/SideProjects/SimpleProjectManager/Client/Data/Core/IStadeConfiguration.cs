using JetBrains.Annotations;
using ReduxSimple;

namespace SimpleProjectManager.Client.Data.Core;

[PublicAPI]
public interface ISelectorFactory<TState>
{
	ISelectorWithoutProps<TState, TFinalResult> CreateSelector<TSelectorResult1, TFinalResult>
		(ISelectorWithoutProps<TState, TSelectorResult1> selector1, Func<TSelectorResult1, TFinalResult> projectorFunction);

	ISelectorWithoutProps<TState, TFinalResult> CreateSelector<TSelectorResult1, TSelectorResult2, TFinalResult>
	(ISelectorWithoutProps<TState, TSelectorResult1> selector1,
		ISelectorWithoutProps<TState, TSelectorResult2> selector2,
		Func<TSelectorResult1, TSelectorResult2, TFinalResult> projectorFunction);

	ISelectorWithProps<TState, TProps, TFinalResult> CreateSelector<TProps, TSelectorResult1, TFinalResult>(
		ISelector<TState, TSelectorResult1> selector1, Func<TSelectorResult1, TProps, TFinalResult> projectorFunction);

	ISelectorWithProps<TState, TProps, TFinalResult> CreateSelector<TProps, TSelectorResult1, TSelectorResult2, TFinalResult>(
		ISelector<TState, TSelectorResult1> selector1,
		ISelector<TState, TSelectorResult2> selector2,
		Func<TSelectorResult1, TSelectorResult2, TProps, TFinalResult> projectorFunction);

	ISelectorWithoutProps<TState, TResult> CreateSelector<TResult>(Func<TState, TResult> selector);
}

[PublicAPI]
public interface IReducerFactory<TState>
	where TState : class, new()
{
	IEnumerable<On<TState>> CombineReducers(params IEnumerable<On<TState>>[] reducersList);

	On<TState> On<TAction>(Func<TState, TAction, TState> reducer) where TAction : class;

	On<TState> On<TAction>(Func<TState, TState> reducer) where TAction : class;

	On<TState> On<TAction1, TAction2>(Func<TState, TState> reducer)
		where TAction1 : class where TAction2 : class;

	On<TState> On<TAction1, TAction2, TAction3>(Func<TState, TState> reducer)
		where TAction1 : class where TAction2 : class where TAction3 : class;

	On<TState> On<TAction1, TAction2, TAction3, TAction4>(Func<TState, TState> reducer)
		where TAction1 : class where TAction2 : class where TAction3 : class where TAction4 : class;

	IStateLens<TState, TFeatureState> CreateSubReducers<TFeatureState>(Func<TState, TFeatureState> featureSelector)
		where TFeatureState : class, new();

	IStateLens<TState, TFeatureState> CreateSubReducers<TAction, TFeatureState>(Func<TState, TAction, TFeatureState> featureSelector)
		where TFeatureState : class, new();

	IStateLens<TState, TFeatureState> CreateSubReducers<TFeatureState>(ISelectorWithoutProps<TState, TFeatureState> featureSelector)
		where TFeatureState : class, new();

	IStateLens<TState, TFeatureState> CreateSubReducers<TFeatureState>(Func<TState, TFeatureState> featureSelector, Func<TState, TFeatureState, TState> stateReducer)
		where TFeatureState : class, new();

	IStateLens<TState, TFeatureState> CreateSubReducers<TAction, TFeatureState>(Func<TState, TAction, TFeatureState> featureSelector, Func<TState, TAction, TFeatureState, TState> stateReducer)
		where TFeatureState : class, new();

	IStateLens<TState, TFeatureState> CreateSubReducers<TFeatureState>(ISelectorWithoutProps<TState, TFeatureState> featureSelector, Func<TState, TFeatureState, TState> stateReducer)
		where TFeatureState : class, new();
}

[PublicAPI]
public interface IEffectFactory<TState>
	where TState : class, new()
{
	IEffect CreateEffect(Func<IObservable<object>> run, bool dispatch = true);

	IEffect CreateEffect(Func<IObservable<TState>, IObservable<object>> run, bool dispatch = true);

	IEffect CreateEffect<TAction>(Func<IObservable<(TAction Action, TState State)>, IObservable<object>> run, bool dispatch = true);
}

[PublicAPI]
public interface IRequestFactory<TState>
{
	IRequestFactory<TState> AddRequest<TAction>(Func<TAction, Task<string>> runRequest, Func<TState, TAction, TState> onScess)
		where TAction : class;
	
	IRequestFactory<TState> AddRequest<TAction>(
		Func<TAction, Task<string>> runRequest, 
		Func<TState, TAction, TState> onScess,
		Func<TState, object, TState> onFail)
		where TAction : class;

	IRequestFactory<TState> OnTheFlyUpdate<TSource, TData>(
		Func<TState, TSource> sourceSelector,
		Func<CancellationToken, Func<CancellationToken, ValueTask<TSource>>, Task<TData>> fetcher,
		Func<TState, TData, TState> patcher);
}

[PublicAPI]
public interface IStateConfiguration<TActualState>
	where TActualState : class, new()
{
    ISelectorFactory<TActualState> SelectorFactory { get; }

    IStateConfiguration<TActualState> ApplyReducer(Func<IReducerFactory<TActualState>, On<TActualState>> factory);

    IStateConfiguration<TActualState> ApplyReducers(params Func<IReducerFactory<TActualState>, On<TActualState>>[] factorys);

    IStateConfiguration<TActualState> ApplyReducers(Func<IReducerFactory<TActualState>, IEnumerable<On<TActualState>>> factory);

    IStateConfiguration<TActualState> ApplyEffect(Func<IEffectFactory<TActualState>, IEffect> factory);

    IStateConfiguration<TActualState> ApplyEffects(params Func<IEffectFactory<TActualState>, IEffect>[] factorys);

    IStateConfiguration<TActualState> ApplyEffects(Func<IEffectFactory<TActualState>, IEnumerable<IEffect>> factory);

    IStateConfiguration<TActualState> ApplyRequests(Action<IRequestFactory<TActualState>> factory);

    IStateConfiguration<TActualState> ApplyRequests(params Action<IRequestFactory<TActualState>>[] factorys);

    IConfiguredState AndFinish();
}
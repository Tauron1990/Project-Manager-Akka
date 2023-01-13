using JetBrains.Annotations;

namespace Tauron.Application.Akka.Redux.Configuration;

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
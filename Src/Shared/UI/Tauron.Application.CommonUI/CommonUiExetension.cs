using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Tauron.Application.CommonUI.AppCore;
using Tauron.Application.CommonUI.Model;

namespace Tauron.Application.CommonUI;

[PublicAPI]
public static class CommonUiExetension
{
    public static async Task RunCommonUi(this IHostBuilder hostBuilder)
    {
        hostBuilder.ConfigureServices(sc => sc.AddSingleton<IHostLifetime, UiAppService>());

        await hostBuilder.Build().RunAsync().ConfigureAwait(false);
    }

    #region UIProperty

    public static FluentPropertyRegistration<TData> WithFlow<TData>(
        this FluentPropertyRegistration<TData> prop,
        Func<IObservable<TData>, IDisposable> flowBuilder)
    {
        var aFlow = new Subject<TData>();

        prop.Actor.AddResource(flowBuilder(aFlow.AsObservable()));
        prop.Actor.AddResource(aFlow);

        return prop.OnChange(d => aFlow.OnNext(d));
    }

    #endregion

    public static IObservable<TData> ObserveOnDispatcher<TData>(this IObservable<TData> observable)
        => observable.ObserveOn(DispatcherScheduler.CurrentDispatcher);

    #region Command

    public static CommandRegistrationBuilder WithParameterFlow<TParameter>(this CommandRegistrationBuilder builder, Func<IObservable<TParameter?>, IDisposable> flowBuilder)
        => WithFlow(builder, o => o switch { TParameter par => par, _ => default }, flowBuilder);

    public static CommandRegistrationBuilder WithFlow(this CommandRegistrationBuilder builder, Func<IObservable<Unit>, IDisposable> flowBuilder)
        => WithFlow(builder, Unit.Default, flowBuilder);

    public static CommandRegistrationBuilder WithFlow<TStart>(
        this CommandRegistrationBuilder builder,
        TStart trigger, Func<IObservable<TStart>, IDisposable> flowBuilder)
        => WithFlow(builder, _ => trigger, flowBuilder);

    public static CommandRegistrationBuilder WithFlow<TStart>(this CommandRegistrationBuilder builder, Func<TStart> trigger, Func<IObservable<TStart>, IDisposable> flowBuilder)
        => WithFlow(builder, _ => trigger(), flowBuilder);

    public static CommandRegistrationBuilder WithFlow<TStart>(this CommandRegistrationBuilder builder, Func<object?, TStart> trigger, Func<IObservable<TStart>, IDisposable> flowBuilder)
    {
        var ob = new Subject<TStart>();
        IDisposable sub = flowBuilder(ob.AsObservable());

        builder.Target.AddResource(ob);
        builder.Target.AddResource(sub);

        return builder.WithExecute(o => ob.OnNext(trigger(o)));
    }

    public static IDisposable ToModel<TRecieve>(this IObservable<TRecieve> selector, IViewModel model)
    {
        return selector.ToActor(() => model.Actor);
    }

    #endregion
}
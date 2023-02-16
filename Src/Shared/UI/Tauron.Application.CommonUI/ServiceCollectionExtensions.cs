using System;
using Akka.Actor;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Tauron.Application.CommonUI.Model;
using Tauron.Application.CommonUI.UI;
using Tauron.TAkka;

namespace Tauron.Application.CommonUI;

[PublicAPI]
public static class ServiceCollectionExtensions
{
    public static IServiceCollection RegisterView<TView, TModel>(this IServiceCollection builder)
        where TView : class, IView
        where TModel : UiActor
    {
        AutoViewLocation.AddPair(typeof(TView), typeof(TModel));

        builder.TryAddScoped<TView>();

        return RegisterModel<TModel>(builder);
    }

    public static IServiceCollection RegisterModel<TModel>(this IServiceCollection builder)
        where TModel : UiActor
        => builder.AddScoped<IViewModel<TModel>, ViewModelActorRef<TModel>>();
}
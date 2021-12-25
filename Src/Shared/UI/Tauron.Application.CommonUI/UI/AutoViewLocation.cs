using System;
using System.Collections.Generic;
using Akka.Util;
using Autofac;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Tauron.AkkaHost;

namespace Tauron.Application.CommonUI.UI;

[PublicAPI]
public sealed class AutoViewLocation
{
    private static readonly Dictionary<Type, Type> Views = new();

    private readonly ILifetimeScope _provider;

    public AutoViewLocation(ILifetimeScope provider) => _provider = provider;

    public static AutoViewLocation Manager => ActorApplication.ServiceProvider.GetRequiredService<AutoViewLocation>();

    public static void AddPair(Type view, Type model)
        => Views[model] = view;

    public Option<IView> ResolveView(object viewModel)
    {
        if (viewModel is not IViewModel model)
            return Option<IView>.None;

        var type = model.ModelType;

        return Views.TryGetValue(type, out var view)
            ? (_provider.ResolveOptional(view, new TypedParameter(typeof(IViewModel<>).MakeGenericType(type), viewModel)) as IView).OptionNotNull()
            : Option<IView>.None;
    }
}
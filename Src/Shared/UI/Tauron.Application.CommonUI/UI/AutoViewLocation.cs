using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Stl;

namespace Tauron.Application.CommonUI.UI;

[PublicAPI]
public sealed class AutoViewLocation
{
    private static readonly Dictionary<Type, Type> Views = new();

    private readonly IServiceProvider _provider;

    public AutoViewLocation(IServiceProvider provider) => _provider = provider;

    public static AutoViewLocation Manager => TauronEnviroment.ServiceProvider.GetRequiredService<AutoViewLocation>();

    public static void AddPair(Type view, Type model)
        => Views[model] = view;

    public Option<IView> ResolveView(object viewModel)
    {
        if(viewModel is not IViewModel model)
            return Option<IView>.None;

        Type type = model.ModelType;

        return Views.TryGetValue(type, out Type? view)
            ? ActivatorUtilities.CreateInstance(_provider, view, viewModel)
               .OptionNotNull()
               .CastAs<IView>()
               .FlatSelect(v => v is null ? Option<IView>.None : Option<IView>.Some(v))
            : Option<IView>.None;
    }
}
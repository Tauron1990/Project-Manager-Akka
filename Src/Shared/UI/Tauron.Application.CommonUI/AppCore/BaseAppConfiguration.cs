using System;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Tauron.Application.CommonUI.AppCore;

[PublicAPI]
public class BaseAppConfiguration
{
    internal readonly IServiceCollection ServiceCollection;

    public BaseAppConfiguration(IServiceCollection serviceCollection) => ServiceCollection = serviceCollection;

    public BaseAppConfiguration WithAppFactory(Func<IUIApplication> factory)
    {
        ServiceCollection.TryAddSingleton<IAppFactory>(_ => new DelegateAppFactory(factory));

        return this;
    }

    //public BaseAppConfiguration WithRoute<TRoute>(string name)
    //    where TRoute : class, IAppRoute
    //{
    //    ServiceCollection.RegisterType<TRoute>().Named<IAppRoute>(name);
    //    return this;
    //}
}
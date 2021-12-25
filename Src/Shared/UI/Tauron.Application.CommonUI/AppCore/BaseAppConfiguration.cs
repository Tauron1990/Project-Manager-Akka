using System;
using Autofac;
using JetBrains.Annotations;

namespace Tauron.Application.CommonUI.AppCore;

[PublicAPI]
public class BaseAppConfiguration
{
    internal readonly ContainerBuilder ServiceCollection;

    public BaseAppConfiguration(ContainerBuilder serviceCollection) => ServiceCollection = serviceCollection;

    public BaseAppConfiguration WithAppFactory(Func<IUIApplication> factory)
    {
        ServiceCollection.Register(_ => new DelegateAppFactory(factory)).As<IAppFactory>()
           .IfNotRegistered(typeof(IAppFactory));

        return this;
    }

    //public BaseAppConfiguration WithRoute<TRoute>(string name)
    //    where TRoute : class, IAppRoute
    //{
    //    ServiceCollection.RegisterType<TRoute>().Named<IAppRoute>(name);
    //    return this;
    //}
}
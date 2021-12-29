using JetBrains.Annotations;
using Tauron.Application.Workshop.StateManagement.Akka.Builder;
using Tauron.Application.Workshop.StateManagement.Akka.Dispatcher.WorkDistributor;
using Tauron.Application.Workshop.StateManagement.Builder;

namespace Tauron.Application.Workshop.StateManagement.Akka;

[PublicAPI]
public static class ManagerBuilderExtensions
{
    public static IConcurrentDispatcherConfugiration WithConcurentDispatcher<TReturn>(this IDispatcherConfigurable<TReturn> builder)
        where TReturn : IDispatcherConfigurable<TReturn>
    {
        var config = new ConcurrentDispatcherConfugiration();
        builder.WithDispatcher(config.Create);

        return config;
    }

    public static IConsistentHashDispatcherPoolConfiguration WithConsistentHashDispatcher<TReturn>(this IDispatcherConfigurable<TReturn> builder)
        where TReturn : IDispatcherConfigurable<TReturn>
    {
        var config = new ConsistentHashDispatcherConfiguration();
        builder.WithDispatcher(config.Create);

        return config;
    }

    public static TType WithWorkDistributorDispatcher<TType>(this TType source, TimeSpan? timeout = null)
        where TType : IDispatcherConfigurable<TType>
    {
        timeout ??= TimeSpan.FromMinutes(1);

        return source.WithDispatcher(() => new WorkDistributorConfigurator(timeout.Value));
    }

    public static IConsistentHashDispatcherPoolConfiguration WithConsistentHashDispatcher<TReturn>(this IDispatcherConfigurable<TReturn> builder, string name)
        where TReturn : IDispatcherConfigurable<TReturn>
    {
        var config = new ConsistentHashDispatcherConfiguration();
        builder.WithDispatcher(name, config.Create);

        return config;
    }

    public static TType WithWorkDistributorDispatcher<TType>(this TType source, string name, TimeSpan? timeout = null)
        where TType : IDispatcherConfigurable<TType>
    {
        timeout ??= TimeSpan.FromMinutes(1);

        return source.WithDispatcher(name, () => new WorkDistributorConfigurator(timeout.Value));
    }


    public static IConcurrentDispatcherConfugiration WithConcurentDispatcher<TReturn>(this IDispatcherConfigurable<TReturn> builder, string name)
        where TReturn : IDispatcherConfigurable<TReturn>
    {
        var config = new ConcurrentDispatcherConfugiration();
        builder.WithDispatcher(name, config.Create);

        return config;
    }
}
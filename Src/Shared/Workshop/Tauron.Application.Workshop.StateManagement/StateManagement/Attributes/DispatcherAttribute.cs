using JetBrains.Annotations;
using Tauron.Application.Workshop.StateManagement.Dispatcher;
using Tauron.Application.Workshop.StateManagement.Dispatcher.WorkDistributor;

namespace Tauron.Application.Workshop.StateManagement.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public abstract class DispatcherAttribute : Attribute
{
    public string? Name { get; set; }

    protected internal abstract Func<IStateDispatcherConfigurator> CreateConfig();
}

[PublicAPI]
public sealed class DefaultDispatcherAttribute : DispatcherAttribute
{
    protected internal override Func<IStateDispatcherConfigurator> CreateConfig() => () => new DefaultStateDispatcher();
}

[PublicAPI]
public sealed class ConsistentHashDispatcherAttribute : DispatcherAttribute
{
    protected internal override Func<IStateDispatcherConfigurator> CreateConfig() => () => new ConsistentHashStateDispatcher();
}

[PublicAPI]
public sealed class ConcurrentDispatcherAttribute : DispatcherAttribute
{
    protected internal override Func<IStateDispatcherConfigurator> CreateConfig() => () => new ConcurrentStateDispatcher();
}

[PublicAPI]
public sealed class WorkDistributorDispatcherAttribute : DispatcherAttribute
{
    public TimeSpan? Timeout { get; set; }

    protected internal override Func<IStateDispatcherConfigurator> CreateConfig() => () => new WorkDistributorConfigurator(Timeout ?? TimeSpan.FromMinutes(1));
}
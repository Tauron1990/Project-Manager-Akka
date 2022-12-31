using JetBrains.Annotations;
using Tauron.Application.Workshop.StateManagement.Dispatcher;

namespace Tauron.Application.Workshop.StateManagement.Attributes;

[AttributeUsage(AttributeTargets.Class), PublicAPI]
public abstract class DispatcherAttribute : Attribute
{
    public string? Name { get; set; }

    protected internal abstract Func<IStateDispatcherConfigurator> CreateConfig();
}
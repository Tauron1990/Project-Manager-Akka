using JetBrains.Annotations;
using Tauron.Application.Workshop.StateManagement.Dispatcher;

namespace Tauron.Application.Workshop.StateManagement.Attributes;

[PublicAPI]
public sealed class DefaultDispatcherAttribute : DispatcherAttribute
{
    protected internal override Func<IStateDispatcherConfigurator> CreateConfig() => () => new DefaultStateDispatcher();
}
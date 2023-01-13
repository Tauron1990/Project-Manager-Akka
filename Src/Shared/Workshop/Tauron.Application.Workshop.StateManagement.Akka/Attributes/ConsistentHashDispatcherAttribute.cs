using JetBrains.Annotations;
using Tauron.Application.Workshop.StateManagement.Akka.Dispatcher;
using Tauron.Application.Workshop.StateManagement.Attributes;
using Tauron.Application.Workshop.StateManagement.Dispatcher;

namespace Tauron.Application.Workshop.StateManagement.Akka.Attributes;

[PublicAPI]
public sealed class ConsistentHashDispatcherAttribute : DispatcherAttribute
{
    protected override Func<IStateDispatcherConfigurator> CreateConfig() => () => new ConsistentHashStateDispatcher();
}
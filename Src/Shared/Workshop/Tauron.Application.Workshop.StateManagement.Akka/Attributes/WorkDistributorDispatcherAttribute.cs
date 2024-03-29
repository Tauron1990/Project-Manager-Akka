using JetBrains.Annotations;
using Tauron.Application.Workshop.StateManagement.Akka.Dispatcher.WorkDistributor;
using Tauron.Application.Workshop.StateManagement.Attributes;
using Tauron.Application.Workshop.StateManagement.Dispatcher;

namespace Tauron.Application.Workshop.StateManagement.Akka.Attributes;

[PublicAPI]
public sealed class WorkDistributorDispatcherAttribute : DispatcherAttribute
{
    public TimeSpan? Timeout { get; set; }

    protected override Func<IStateDispatcherConfigurator> CreateConfig() => () => new WorkDistributorConfigurator(Timeout ?? TimeSpan.FromMinutes(1));
}
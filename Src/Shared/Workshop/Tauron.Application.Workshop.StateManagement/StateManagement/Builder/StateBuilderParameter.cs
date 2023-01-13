using Tauron.Application.Workshop.Mutation;
using Tauron.Application.Workshop.StateManagement.StatePooling;

namespace Tauron.Application.Workshop.StateManagement.Builder;

public sealed record StateBuilderParameter(
    MutatingEngine Engine, IServiceProvider? ServiceProvider, IActionInvoker Invoker, StatePool StatePool, DispatcherPool DispatcherPool,
    IStateInstanceFactory[] InstanceFactoys);
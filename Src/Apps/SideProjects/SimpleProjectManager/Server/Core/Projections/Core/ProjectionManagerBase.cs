using Akka.Actor;
using Akkatecture.Aggregates;
using Akkatecture.Core;
using SimpleProjectManager.Server.Core.Data;
using Tauron.Akkatecture.Projections;

namespace SimpleProjectManager.Server.Core.Projections.Core;

public abstract class ProjectionManagerBase : IDisposable
{
    private IDisposable _subscription = System.Reactive.Disposables.Disposable.Empty;

    protected void InitializeDispatcher<TProjection, TAggregate, TIdentity>(
        ActorSystem system, Action<DomainEventMapBuilder<TProjection, TAggregate, TIdentity>> mapBuilderAction)
        where TProjection : class, IProjectorData<TIdentity>
        where TAggregate : IAggregateRoot<TIdentity>
        where TIdentity : IIdentity
    {
        var dispatcher = ProjectionBuilder.CreateProjection(system, mapBuilderAction);

        _subscription = dispatcher.Subscribe<TAggregate>();
    }

    public virtual void Dispose()
        => _subscription.Dispose();
}
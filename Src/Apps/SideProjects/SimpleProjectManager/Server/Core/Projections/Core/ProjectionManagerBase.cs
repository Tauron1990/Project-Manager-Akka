using System.Reactive.Disposables;
using Akka.Actor;
using Akkatecture.Aggregates;
using Akkatecture.Core;
using Tauron.Akkatecture.Projections;

namespace SimpleProjectManager.Server.Core.Projections.Core;

public abstract class ProjectionManagerBase : IDisposable
{
    private readonly ILoggerFactory _loggerFactory;
    private IDisposable _subscription = Disposable.Empty;


    protected ProjectionManagerBase(ILoggerFactory loggerFactory)
        => _loggerFactory = loggerFactory;

    public virtual void Dispose()
        => _subscription.Dispose();

    protected void InitializeDispatcher<TProjection, TAggregate, TIdentity>(
        ActorSystem system, Action<DomainEventMapBuilder<TProjection, TAggregate, TIdentity>> mapBuilderAction)
        where TProjection : class, IProjectorData<TIdentity>
        where TAggregate : IAggregateRoot<TIdentity>
        where TIdentity : IIdentity
    {
        var dispatcher = ProjectionBuilder.CreateProjection(system, mapBuilderAction, _loggerFactory);

        _subscription = dispatcher.Subscribe<TAggregate>();
    }
}
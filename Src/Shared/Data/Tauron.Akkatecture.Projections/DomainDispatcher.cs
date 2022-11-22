using System;
using System.Threading.Tasks;
using Akka.Streams;
using Akkatecture.Core;
using JetBrains.Annotations;
using LiquidProjections;
using LiquidProjections.Abstractions;
using Microsoft.Extensions.Logging;

namespace Tauron.Akkatecture.Projections;

[PublicAPI]
public class DomainDispatcher<TProjection, TIdentity>
    where TProjection : class, IProjectorData<TIdentity>
    where TIdentity : IIdentity
{
    private readonly ILogger<DomainDispatcher<TProjection, TIdentity>> _logger;
    protected internal readonly Dispatcher Dispatcher;

    public DomainDispatcher(AggregateEventReader reader, DomainProjector projector, IProjectionRepository repo, ILogger<DomainDispatcher<TProjection, TIdentity>> logger)
    {
        _logger = logger;
        Reader = reader;
        Projector = projector;
        Repo = repo;

        reader.OnReadError += (_, exception)
                                  =>
                              {
                                  if(exception.Exception is AbruptTerminationException) return;

                                  logger.LogError(
                                      exception.Exception,
                                      "Error while Processing Journal Events: {ProjectionType} - {Tag}",
                                      typeof(TProjection).Name,
                                      exception.Tag);
                              };
        Dispatcher = new Dispatcher(reader.CreateSubscription)
                     {
                         ExceptionHandler = ExceptionHandler,
                         SuccessHandler = SuccessHandler
                     };
    }

    public AggregateEventReader Reader { get; }
    public DomainProjector Projector { get; }
    public IProjectionRepository Repo { get; }

    protected virtual Task SuccessHandler(SubscriptionInfo info) => Task.CompletedTask;

    protected virtual Task<ExceptionResolution> ExceptionHandler(Exception exception, int attempts, SubscriptionInfo info)
    {
        _logger.LogError(
            exception,
            "Error while Processing Event on Projection: {ProjectionType} - {SubscriptionId} - {Attempts}",
            typeof(TProjection).Name,
            info.Id,
            attempts);

        return !exception.IsCriticalApplicationException() && attempts < 3
            ? Task.FromResult(ExceptionResolution.Retry)
            : Task.FromResult(ExceptionResolution.Abort);
    }

    public IDisposable Subscribe<TAggregate>()
    {
        var options = new SubscriptionOptions { Id = "Type@" + typeof(TAggregate).AssemblyQualifiedName };

        return Dispatcher.Subscribe(
            Repo.GetLastCheckpoint<TProjection, TIdentity>(),
            (list, _) => Projector.Projector.Handle(list),
            options);
    }

    public IDisposable Subscribe(string tag)
    {
        var options = new SubscriptionOptions { Id = "Tag@" + tag };

        return Dispatcher.Subscribe(
            Repo.GetLastCheckpoint<TProjection, TIdentity>(),
            (list, _) => Projector.Projector.Handle(list),
            options);
    }
}
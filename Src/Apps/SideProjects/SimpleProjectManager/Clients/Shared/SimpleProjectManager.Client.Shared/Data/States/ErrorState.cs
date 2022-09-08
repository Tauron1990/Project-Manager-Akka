using System;
using System.Reactive.Linq;
using SimpleProjectManager.Client.Shared.Data.States.Actions;
using SimpleProjectManager.Client.Shared.Data.States.Data;
using SimpleProjectManager.Shared.Services;
using Stl.Fusion;
using Tauron.Applicarion.Redux;

namespace SimpleProjectManager.Client.Shared.Data.States;

public sealed class ErrorState : StateBase<InternalErrorState>
{
    private readonly ICriticalErrorService _errorService;

    public ErrorState(IStateFactory stateFactory, ICriticalErrorService errorService)
        : base(stateFactory)
        => _errorService = errorService;

    protected override IStateConfiguration<InternalErrorState> ConfigurateState(ISourceConfiguration<InternalErrorState> configuration)
    {
        return configuration
           .FromCacheAndServer(_errorService.CountErrors, (state, errorCount) => state with { ErrorCount = errorCount })
           .ApplyRequests(
                factory =>
                {
                    factory.AddRequest(
                        ErrorStateRequests.WriteError(_errorService),
                        (state, _) => state with { ErrorCount = state.ErrorCount + 1 });

                    factory.AddRequest(
                        ErrorStateRequests.DisableError(_errorService),
                        ErrorStatePatcher.DecrementErrorCount);
                });
    }

    protected override void PostConfiguration(IRootStoreState<InternalErrorState> state)
    {
        Errors = FromServer(_errorService.GetErrors);
        ErrorCount = state.Select(data => data.ErrorCount);
        //ErrorCount.Subscribe(data => Console.WriteLine($"Critical Error Count: {data}"));
    }

    public IObservable<long> ErrorCount { get; private set; } = Observable.Empty<long>();

    public IObservable<CriticalError[]> Errors { get; private set; } = Observable.Empty<CriticalError[]>();
} 
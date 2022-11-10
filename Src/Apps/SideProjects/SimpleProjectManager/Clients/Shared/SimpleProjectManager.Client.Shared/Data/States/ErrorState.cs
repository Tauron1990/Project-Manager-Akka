using System;
using System.Reactive.Linq;
using SimpleProjectManager.Client.Shared.Data.States.Actions;
using SimpleProjectManager.Client.Shared.Data.States.Data;
using SimpleProjectManager.Shared.Services;
using Stl.Fusion;
using Tauron.Applicarion.Redux;
using Tauron.Applicarion.Redux.Configuration;

namespace SimpleProjectManager.Client.Shared.Data.States;

public sealed class ErrorState : StateBase<InternalErrorState>
{
    private readonly ICriticalErrorService _errorService;

    public ErrorState(IStateFactory stateFactory, ICriticalErrorService errorService)
        : base(stateFactory)
        => _errorService = errorService;

    public IObservable<ErrorCount> ErrorCount { get; private set; } = Observable.Empty<ErrorCount>();

    public IObservable<CriticalErrorList> Errors { get; private set; } = Observable.Empty<CriticalErrorList>();

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
}
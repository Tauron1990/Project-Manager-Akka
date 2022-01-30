using System.Reactive.Linq;
using SimpleProjectManager.Shared.Services;
using Stl.Fusion;
using Tauron.Applicarion.Redux;
using Tauron.Applicarion.Redux.Configuration;

namespace SimpleProjectManager.Client.Data.States;

public sealed class ErrorState : StateBase<InternalErrorState>
{
    private readonly ICriticalErrorService _errorService;

    public ErrorState(IStoreConfiguration configuration, IStateFactory stateFactory)
        : base(configuration, stateFactory)
        => _errorService = stateFactory.Services.GetRequiredService<ICriticalErrorService>();

    protected override IStateConfiguration<InternalErrorState> ConfigurateState(ISourceConfiguration<InternalErrorState> configuration)
    {
        return configuration
           .FromServer(_errorService.CountErrors, (state, errorCount) => state with { ErrorCount = errorCount })
           .ApplyRequests(
                factory =>
                {
                    factory.AddRequest(
                        ErrorStateRequests.WriteError(_errorService),
                        (state, _) => state with { ErrorCount = state.ErrorCount + 1 });
                });
    }

    protected override void PostConfiguration(IRootStoreState<InternalErrorState> state)
    {
        Errors = FromServer(_errorService.GetErrors);
        ErrorCount = state.Select(data => data.ErrorCount);
    }

    public IObservable<long> ErrorCount { get; private set; } = Observable.Empty<long>();

    public IObservable<CriticalError[]> Errors { get; private set; } = Observable.Empty<CriticalError[]>();
} 
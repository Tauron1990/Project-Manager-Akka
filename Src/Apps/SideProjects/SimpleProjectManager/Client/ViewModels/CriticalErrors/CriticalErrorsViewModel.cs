using SimpleProjectManager.Shared.Services;
using Stl.Fusion;
using Tauron.Application.Blazor;

namespace SimpleProjectManager.Client.ViewModels;

public sealed class CriticalErrorsViewModel : StatefulViewModel<CriticalError[]>
{
    private readonly ICriticalErrorService _errorService;

    public CriticalErrorsViewModel(IStateFactory stateFactory, ICriticalErrorService errorService) 
        : base(stateFactory)
        => _errorService = errorService;

    protected override async Task<CriticalError[]> ComputeState(CancellationToken cancellationToken)
        => await _errorService.GetErrors(cancellationToken);
}
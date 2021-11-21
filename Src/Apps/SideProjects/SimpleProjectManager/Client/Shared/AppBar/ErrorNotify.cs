using Stl.Fusion.Blazor;

namespace SimpleProjectManager.Client.Shared.AppBar;

public partial class ErrorNotify
{
    protected override async Task<long> ComputeState(CancellationToken cancellationToken)
        => await _errorService.CountErrors(cancellationToken);
}
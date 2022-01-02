using SimpleProjectManager.Client.Data.Core;
using SimpleProjectManager.Shared.Services;

namespace SimpleProjectManager.Client.Data.States;

public sealed class ErrorState : StateBase<InternalErrorState>
{
    private ICriticalErrorService _errorService;

    public ErrorState(IStoreConfiguration configuration, IServiceProvider serviceProvider)
        : base(configuration)
        => _errorService = serviceProvider.GetRequiredService<ICriticalErrorService>();

    protected override IStateConfiguration<InternalErrorState> ConfigurateState(ISourceConfiguration<InternalErrorState> configuration)
    {
        
    }
}
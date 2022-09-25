using SimpleProjectManager.Client.Shared.CriticalErrors;
using SimpleProjectManager.Client.Shared.Data;
using SimpleProjectManager.Client.Shared.Services;
using SimpleProjectManager.Client.Shared.ViewModels.CriticalErrors;
using SimpleProjectManager.Shared.Services;
using Stl.Fusion;
using Tauron.Application.Blazor.Parameters;

namespace SimpleProjectManager.Client.ViewModels;

public sealed class CriticalErrorViewModel : CriticalErrorViewModelBase, IParameterUpdateable
{
    private readonly IStateFactory _stateFactory;

    public CriticalErrorViewModel(IStateFactory stateFactory, GlobalState globalState, IMessageDispatcher messageDispatcher)
        : base(globalState, messageDispatcher)
        => _stateFactory = stateFactory;

    public ParameterUpdater Updater { get; } = new();
    
    protected override IState<CriticalError?> GetErrorState()
        => Updater.Register<CriticalError?>(nameof(CriticalErrorDispaly.Error), _stateFactory);
}
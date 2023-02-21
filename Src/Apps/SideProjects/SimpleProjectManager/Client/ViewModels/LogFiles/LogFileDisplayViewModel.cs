using SimpleProjectManager.Client.Shared.ViewModels;
using Stl.Fusion;
using Tauron.Application.Blazor;

namespace SimpleProjectManager.Client.ViewModels.LogFiles;

public  sealed class LogFileDisplayViewModel : BlazorViewModel
{
    private IState<TargetFileSelection> _input;

    public LogFileDisplayViewModel(IStateFactory stateFactory)
        : base(stateFactory)
    {
    }
}
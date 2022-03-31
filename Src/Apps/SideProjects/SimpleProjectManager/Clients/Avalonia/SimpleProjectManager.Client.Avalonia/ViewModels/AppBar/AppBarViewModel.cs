using SimpleProjectManager.Client.Shared.ViewModels;

namespace SimpleProjectManager.Client.Avalonia.ViewModels.AppBar;

public sealed class AppBarViewModel : ViewModelBase
{
    public NotifyErrorModel ErrorModel { get; }

    public ClockDisplayViewModel ClockModel { get; }
    
    public AppBarViewModel(NotifyErrorModel errorModel, ClockDisplayViewModel clockModel)
    {
        ErrorModel = errorModel;
        ClockModel = clockModel;
    }
}
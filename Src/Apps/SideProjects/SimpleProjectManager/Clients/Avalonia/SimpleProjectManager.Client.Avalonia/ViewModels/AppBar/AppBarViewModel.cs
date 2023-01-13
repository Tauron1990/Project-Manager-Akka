using SimpleProjectManager.Client.Shared.ViewModels;

namespace SimpleProjectManager.Client.Avalonia.ViewModels.AppBar;

public sealed class AppBarViewModel : ViewModelBase
{
    public AppBarViewModel(NotifyErrorModel errorModel, ClockDisplayViewModel clockModel)
    {
        ErrorModel = errorModel;
        ClockModel = clockModel;
    }

    public NotifyErrorModel ErrorModel { get; }

    public ClockDisplayViewModel ClockModel { get; }
}
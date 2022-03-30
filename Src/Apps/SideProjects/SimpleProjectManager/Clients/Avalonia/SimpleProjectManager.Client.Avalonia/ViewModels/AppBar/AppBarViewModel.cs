using SimpleProjectManager.Client.Shared.ViewModels;

namespace SimpleProjectManager.Client.Avalonia.ViewModels.AppBar;

public sealed class AppBarViewModel : ViewModelBase
{
    public NotifyErrorModel ErrorModel { get; }

    public AppBarViewModel(NotifyErrorModel errorModel)
        => ErrorModel = errorModel;
}
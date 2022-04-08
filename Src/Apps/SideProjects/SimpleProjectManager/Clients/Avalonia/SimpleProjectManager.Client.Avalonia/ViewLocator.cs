using Avalonia.Controls;
using Avalonia.Controls.Templates;
using SimpleProjectManager.Client.Avalonia.ViewModels;
using SimpleProjectManager.Client.Avalonia.ViewModels.AppBar;
using SimpleProjectManager.Client.Avalonia.Views;
using SimpleProjectManager.Client.Avalonia.Views.AppBar;
using SimpleProjectManager.Client.Avalonia.Views.CriticalErrors;
using SimpleProjectManager.Client.Shared.ViewModels;
using SimpleProjectManager.Client.Shared.ViewModels.CriticalErrors;

namespace SimpleProjectManager.Client.Avalonia
{
    public class ViewLocator : IDataTemplate
    {
        public IControl Build(object data)
            => data switch
            {
                ClockDisplayViewModel clockDisplayViewModel => new ClockDisplay { DataContext = clockDisplayViewModel },
                AppBarViewModel appBarViewModel => new MainAppBar { DataContext = appBarViewModel },
                NotifyErrorModel notifyErrorModel => new ErrorNotify { DataContext = notifyErrorModel },
                NotFoundViewModel notFoundViewModel => new NotFoundView { DataContext = notFoundViewModel },
                CriticalErrorsViewModel criticalErrorsViewModel => new CriticalErrorsView { DataContext = criticalErrorsViewModel },
                _ => new TextBlock { Text = $"View not Found for {data.GetType().Name}"}
            };

        public bool Match(object data)
        {
            return data is ViewModelBase;
        }
    }
}
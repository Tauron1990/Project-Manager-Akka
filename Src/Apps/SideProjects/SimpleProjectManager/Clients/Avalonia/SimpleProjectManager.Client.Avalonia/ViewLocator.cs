using Avalonia.Controls;
using Avalonia.Controls.Templates;
using SimpleProjectManager.Client.Avalonia.ViewModels.AppBar;
using SimpleProjectManager.Client.Avalonia.Views.AppBar;
using SimpleProjectManager.Client.Shared.ViewModels;

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
                _ => new TextBlock { Text = $"View not Found for {data.GetType().Name}"}
            };

        public bool Match(object data)
        {
            return data is ViewModelBase;
        }
    }
}
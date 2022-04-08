using System.Reactive.Disposables;
using Avalonia.ReactiveUI;
using ReactiveUI;
using SimpleProjectManager.Client.Avalonia.ViewModels;

namespace SimpleProjectManager.Client.Avalonia.Views
{
    public partial class MainWindow : ReactiveWindow<MainWindowViewModel>
    {
        public MainWindow()
        {
            InitializeComponent();

            this.WhenActivated(
                d =>
                {
                    this.OneWayBind(ViewModel, m => m.AppBarModel, v => v.MainAppBar.Content).DisposeWith(d);
                    this.OneWayBind(ViewModel, m => m.CurrentContent, v => v.MainContent.Content).DisposeWith(d);
                });
        }
    }
}
using Avalonia.Controls;
using Avalonia.ReactiveUI;
using SimpleProjectManager.Client.Avalonia.ViewModels;

namespace SimpleProjectManager.Client.Avalonia.Views
{
    public partial class MainWindow : ReactiveWindow<MainWindowViewModel>
    {
        public MainWindow()
        {
            InitializeComponent();

            Closed += (_, _) => ViewModel?.Dispose();
        }
    }
}
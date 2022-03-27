using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;
using SimpleProjectManager.Client.Avalonia.Models.Data;
using SimpleProjectManager.Client.Avalonia.ViewModels;
using SimpleProjectManager.Client.Avalonia.Views;
using Splat;
using Tauron;

namespace SimpleProjectManager.Client.Avalonia
{
    public partial class App : Application
    {
        private ServiceProvider? _serviceProvider;

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                CreateSeriveProvider();

                desktop.Exit += FreeServiceProvider;
                desktop.MainWindow = new MainWindow
                                     {
                                         ViewModel = _serviceProvider?.GetService<MainWindowViewModel>(),
                                     };
            }

            base.OnFrameworkInitializationCompleted();
        }

        private void FreeServiceProvider(object? sender, ControlledApplicationLifetimeExitEventArgs e)
            => _serviceProvider?.Dispose();

        private void CreateSeriveProvider()
        {
            var collection = new ServiceCollection();
            
            collection.RegisterModule<InternalDataModule>();
            collection.AddTransient<MainWindowViewModel>();
            
            _serviceProvider = collection.BuildServiceProvider();
        }
    }
}
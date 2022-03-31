using System;
using System.Reactive.Disposables;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using SimpleProjectManager.Client.Avalonia.Models.Services;
using SimpleProjectManager.Client.Avalonia.ViewModels;
using SimpleProjectManager.Client.Avalonia.Views;
using SimpleProjectManager.Client.Shared.ViewModels;
using SimpleProjectManager.Shared.ServerApi;
using Tauron;

namespace SimpleProjectManager.Client.Avalonia
{
    // ReSharper disable once PartialTypeWithSinglePart
    public partial class App : Application
    {
        private ServiceProvider? _serviceProvider;

        public static IDisposable Disposer { get; private set; } = Disposable.Empty;
        
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                CreateSeriveProvider();
                
                desktop.MainWindow = new MainWindow
                                     {
                                         ViewModel = _serviceProvider?.GetService<MainWindowViewModel>(),
                                     };
            }

            base.OnFrameworkInitializationCompleted();
        }

        private void CreateSeriveProvider()
        {
            var collection = new ServiceCollection();
            
            collection.RegisterModule<InternalDataModule>();
            collection.RegisterModule<InternalViewModelModule>();
            collection.RegisterModule<ViewModelModule>();
            
            ClientRegistration.ConfigFusion(collection, new Uri("http://localhost:4001"));
            
            _serviceProvider = collection.BuildServiceProvider();
            Disposer = _serviceProvider;
        }
    }
}
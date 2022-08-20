using System;
using System.IO;
using System.Reactive.Disposables;
using Akka.Configuration;
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

                desktop.Exit += (_, _) => _serviceProvider?.Dispose();
            }

            base.OnFrameworkInitializationCompleted();
        }

        private void CreateSeriveProvider()
        {
            var collection = new ServiceCollection();
            
            collection.RegisterModule<InternalDataModule>();
            collection.RegisterModule<InternalViewModelModule>();
            collection.RegisterModule<ViewModelModule>();
            
            string ip = "http://localhost:4000";

            if (File.Exists("seed.conf"))
            {
                var config = ConfigurationFactory.ParseString(File.ReadAllText("seed.conf"));
                ip = $"http://{config.GetString("akka.remote.dot-netty.tcp.hostname")}:4000";

                //await SetupRunner.Run(ip);
            }
            
            ClientRegistration.ConfigFusion(collection, new Uri(ip));
            
            _serviceProvider = collection.BuildServiceProvider();
            Disposer = _serviceProvider;
        }
    }
}
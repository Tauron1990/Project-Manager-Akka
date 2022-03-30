using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Material.Styles;
using Material.Styles.Models;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;
using SimpleProjectManager.Client.Avalonia.Controls;
using SimpleProjectManager.Client.Avalonia.Models;
using SimpleProjectManager.Client.Avalonia.ViewModels.AppBar;
using SimpleProjectManager.Client.Shared.ViewModels;
using Tauron.Application;

namespace SimpleProjectManager.Client.Avalonia.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public AppBarViewModel AppBarModel { get; }
        
        [ActivatorUtilitiesConstructor]
        public MainWindowViewModel(IEventAggregator aggregator, AppBarViewModel appBarViewModel)
        {
            AppBarModel = appBarViewModel;
            
            this.WhenActivated(
                dispo =>
                {
                    aggregator.ConsumeSharedMessage()
                       .ObserveOn(RxApp.MainThreadScheduler)
                       .Subscribe(m =>
                                  {
                                      var alert = new Alert
                                                  {
                                                      Message = m.Message,
                                                      Severity = m.MessageType switch
                                                      {
                                                          MessageType.Info => AlertSeverity.Information,
                                                          MessageType.Warning => AlertSeverity.Warning,
                                                          MessageType.Error => AlertSeverity.Error,
                                                          _ => AlertSeverity.Error
                                                      }
                                                  };
                                      SnackbarHost.Post(new SnackbarModel(alert));
                                  })
                       .DisposeWith(dispo);
                });
        }
    }
}
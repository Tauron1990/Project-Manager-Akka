using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Material.Styles;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;
using SimpleProjectManager.Client.Avalonia.Models;
using SimpleProjectManager.Client.Shared.ViewModels;
using Tauron.Application;

namespace SimpleProjectManager.Client.Avalonia.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public MainWindowViewModel()
        {
            
        }
        
        [ActivatorUtilitiesConstructor]
        public MainWindowViewModel(IEventAggregator aggregator)
        {
            this.WhenActivated(
                dispo =>
                {
                    aggregator.ConsumeSharedMessage()
                       .ObserveOn(RxApp.MainThreadScheduler)
                       .Subscribe(m => SnackbarHost.Post("Test"))
                       .DisposeWith(dispo);
                });
        }
    }
}
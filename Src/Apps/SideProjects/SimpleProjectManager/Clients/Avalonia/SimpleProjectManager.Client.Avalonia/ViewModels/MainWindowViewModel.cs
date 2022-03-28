using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using Material.Styles;
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
        []
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
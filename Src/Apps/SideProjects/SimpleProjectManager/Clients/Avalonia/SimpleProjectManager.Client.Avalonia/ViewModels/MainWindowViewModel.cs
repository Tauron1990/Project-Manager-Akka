using System;
using System.Collections.Generic;
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

namespace SimpleProjectManager.Client.Avalonia.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private ObservableAsPropertyHelper<ViewModelBase?>? _currentContent;


    [ActivatorUtilitiesConstructor]
    public MainWindowViewModel(IEventAggregator aggregator, AppBarViewModel appBarViewModel)
    {
        AppBarModel = appBarViewModel;
        this.WhenActivated(Init);

        IEnumerable<IDisposable> Init()
        {
            yield return aggregator.ConsumeSharedMessage()
               .ObserveOn(RxApp.MainThreadScheduler)
               .Subscribe(
                    m =>
                    {
                        (string message, MessageType messageType) = m;
                        var alert = new Alert
                                    {
                                        Message = message,
                                        Severity = messageType switch
                                        {
                                            MessageType.Info => AlertSeverity.Information,
                                            MessageType.Warning => AlertSeverity.Warning,
                                            MessageType.Error => AlertSeverity.Error,
                                            _ => AlertSeverity.Error
                                        }
                                    };
                        SnackbarHost.Post(new SnackbarModel(alert));
                    });

            yield return _currentContent = aggregator
               .GetEvent<NavigatingEvent, Navigating>()
               .Get()
               .Select(e => e.Model)
               .ToProperty(this, m => m.CurrentContent);
        }
    }

    public AppBarViewModel AppBarModel { get; }
    public ViewModelBase? CurrentContent => _currentContent?.Value;
}
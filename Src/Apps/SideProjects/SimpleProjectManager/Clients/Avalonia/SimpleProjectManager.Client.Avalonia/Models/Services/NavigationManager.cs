using System;
using Microsoft.Extensions.DependencyInjection;
using SimpleProjectManager.Client.Avalonia.ViewModels;
using SimpleProjectManager.Client.Shared.Services;
using SimpleProjectManager.Client.Shared.ViewModels;
using SimpleProjectManager.Client.Shared.ViewModels.CriticalErrors;
using Tauron.Application;

namespace SimpleProjectManager.Client.Avalonia.Models.Services;

public sealed class NavigationManager : INavigationHelper
{
    private readonly IEventAggregator _aggregator;
    private readonly IServiceProvider _serviceProvider;

    public NavigationManager(IEventAggregator aggregator, IServiceProvider serviceProvider)
    {
        _aggregator = aggregator;
        _serviceProvider = serviceProvider;
    }

    public void NavigateTo(string path)
    {
        var evt = _aggregator.GetEvent<NavigatingEvent, Navigating>();

        void Send<TModel>(Action<TModel>? config = null) where TModel : ViewModelBase
        {
            var model = _serviceProvider.GetRequiredService<TModel>();
            config?.Invoke(model);
            
            evt.Publish(new Navigating(model));
        }
        
        switch (path)
        {
            case PageNavigation.CriticalErrorsUrl:
                Send<CriticalErrorsViewModel>();
                break;
            default:
                Send<NotFoundViewModel>();
                break;
        }
    }
}
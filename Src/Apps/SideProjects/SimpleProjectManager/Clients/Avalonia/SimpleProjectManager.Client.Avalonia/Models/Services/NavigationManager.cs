using System;
using SimpleProjectManager.Client.Shared.Services;
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
        
    }
}
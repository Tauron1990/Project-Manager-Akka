using SimpleProjectManager.Client.Shared.Services;
using Tauron.Application;

namespace SimpleProjectManager.Client.Avalonia.Models.Services;

public sealed class NavigationManager : INavigationHelper
{
    private readonly IEventAggregator _aggregator;

    public NavigationManager(IEventAggregator aggregator)
        => _aggregator = aggregator;

    public void NavigateTo(string path)
    {
        
    }
}
using Microsoft.AspNetCore.Components;
using SimpleProjectManager.Client.Shared.Services;

namespace SimpleProjectManager.Client.Data;

public class NavigationHelper : INavigationHelper
{
    private readonly NavigationManager _navigationManager;

    public NavigationHelper(NavigationManager navigationManager)
        => _navigationManager = navigationManager;

    public void NavigateTo(string path)
        => _navigationManager.NavigateTo(path);
}
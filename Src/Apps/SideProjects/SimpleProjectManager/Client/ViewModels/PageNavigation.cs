using Microsoft.AspNetCore.Components;
using SimpleProjectManager.Shared;

namespace SimpleProjectManager.Client.ViewModels;

public class PageNavigation
{
    private readonly NavigationManager _navigationManager;

    public PageNavigation(NavigationManager navigationManager)
    {
        _navigationManager = navigationManager;
    }

    public void NewJob()
        => _navigationManager.NavigateTo("/NewJob");

    public void EditJob(ProjectId id)
        => _navigationManager.NavigateTo($"/EditJob/{id.Value}");
}
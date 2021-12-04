using Microsoft.AspNetCore.Components;
using SimpleProjectManager.Shared;

namespace SimpleProjectManager.Client.ViewModels;

public class PageNavigation
{
    public const string StartPageUrl = "/";
    public const string NewJobUrl = "/NewJob";
    public const string CriticalErrorsUrl = "/CriticalErrors";
    public const string TaskManagerUrl = "/TaskManager";
    public const string FileManagerUrl = "/FileManager";
    
    private readonly NavigationManager _navigationManager;

    public PageNavigation(NavigationManager navigationManager)
        => _navigationManager = navigationManager;

    public void Errors()
        => _navigationManager.NavigateTo(CriticalErrorsUrl);

    public void ShowStartPage()
        => _navigationManager.NavigateTo(StartPageUrl);
    
    public void NewJob()
        => _navigationManager.NavigateTo(NewJobUrl);

    public void EditJob(ProjectId id)
        => _navigationManager.NavigateTo($"/EditJob/{id.Value}");
}
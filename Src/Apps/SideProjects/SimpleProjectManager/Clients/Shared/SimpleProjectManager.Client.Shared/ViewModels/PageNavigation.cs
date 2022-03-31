using SimpleProjectManager.Client.Shared.Services;
using SimpleProjectManager.Shared;

namespace SimpleProjectManager.Client.Shared.ViewModels;

public class PageNavigation
{
    public const string StartPageUrl = "/";
    public const string NewJobUrl = "/NewJob";
    public const string CriticalErrorsUrl = "/CriticalErrors";
    public const string TaskManagerUrl = "/TaskManager";
    public const string FileManagerUrl = "/FileManager";
    public const string CurrentJobs = "/CurrentJobs";
    public const string UploadFilesUrl = "/Upload";
    public const string EditJobUrl = "/EditJob";
    
    public INavigationHelper NavigationManager { get; }
    
    public PageNavigation(INavigationHelper navigationManager)
        => NavigationManager = navigationManager;

    public void ShowCurrentJobs()
        => NavigationManager.NavigateTo(CurrentJobs);

    public void UploadFiles()
        => NavigationManager.NavigateTo(UploadFilesUrl);
    
    public void Errors()
        => NavigationManager.NavigateTo(CriticalErrorsUrl);

    public void ShowStartPage()
        => NavigationManager.NavigateTo(StartPageUrl);
    
    public void NewJob()
        => NavigationManager.NavigateTo(NewJobUrl);

    public void EditJob(ProjectId id)
        => NavigationManager.NavigateTo($"{EditJobUrl}/{id.Value}");
}
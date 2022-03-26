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
    
    private readonly INavigationHelper _navigationManager;

    public PageNavigation(INavigationHelper navigationManager)
        => _navigationManager = navigationManager;

    public void ShowCurrentJobs()
        => _navigationManager.NavigateTo(CurrentJobs);

    public void UploadFiles()
        => _navigationManager.NavigateTo(UploadFilesUrl);
    
    public void Errors()
        => _navigationManager.NavigateTo(CriticalErrorsUrl);

    public void ShowStartPage()
        => _navigationManager.NavigateTo(StartPageUrl);
    
    public void NewJob()
        => _navigationManager.NavigateTo(NewJobUrl);

    public void EditJob(ProjectId id)
        => _navigationManager.NavigateTo($"{EditJobUrl}/{id.Value}");
}
using System.Collections.Generic;
using System.Collections.Immutable;
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
    public const string DevicesUrl = "/Devices";
    public const string LogFilesUrl = "/LogFiles";
    
    #pragma warning disable GU0023
    public static readonly ImmutableList<string> All = ImmutableList<string>.Empty.AddRange(GetAll());
    #pragma warning restore GU0023

    public PageNavigation(INavigationHelper navigationManager)
        => NavigationManager = navigationManager;

    public INavigationHelper NavigationManager { get; }

    private static IEnumerable<string> GetAll()
    {
        yield return NewJobUrl;
        yield return CriticalErrorsUrl;
        yield return TaskManagerUrl;
        yield return FileManagerUrl;
        yield return CurrentJobs;
        yield return UploadFilesUrl;
        yield return EditJobUrl;
        yield return DevicesUrl;
        yield return LogFilesUrl;
    }

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
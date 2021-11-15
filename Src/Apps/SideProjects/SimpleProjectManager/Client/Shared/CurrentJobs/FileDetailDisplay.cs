using Microsoft.AspNetCore.Components;
using ReactiveUI;
using SimpleProjectManager.Client.ViewModels;
using SimpleProjectManager.Shared;
using Tauron;
using Tauron.Application.Blazor;

namespace SimpleProjectManager.Client.Shared.CurrentJobs;

public partial class FileDetailDisplay
{
    [Parameter]
    public ProjectFileId? FileId { get; set; }

    protected override FileDetailDisplayViewModel CreateModel()
        => Services.GetIsolatedService<FileDetailDisplayViewModel>().DisposeWith(this);
}
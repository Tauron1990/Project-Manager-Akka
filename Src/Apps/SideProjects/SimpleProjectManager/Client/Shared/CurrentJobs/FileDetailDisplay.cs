using Microsoft.AspNetCore.Components;
using SimpleProjectManager.Shared;

namespace SimpleProjectManager.Client.Shared.CurrentJobs;

public partial class FileDetailDisplay
{
    [Parameter]
    public ProjectFileId? FileId { get; set; }
}
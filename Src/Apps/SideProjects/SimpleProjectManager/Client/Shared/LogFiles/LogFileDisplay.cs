using Microsoft.AspNetCore.Components;
using SimpleProjectManager.Client.ViewModels.LogFiles;

namespace SimpleProjectManager.Client.Shared.LogFiles;

public sealed partial class LogFileDisplay
{
    [Parameter]
    public TargetFile? ToDisplay { get; set; }
}
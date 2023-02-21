using Microsoft.AspNetCore.Components;
using SimpleProjectManager.Client.ViewModels.LogFiles;
using Tauron.Application.Blazor;

namespace SimpleProjectManager.Client.Shared.LogFiles;

public sealed partial class LogFileDisplay
{
    [Parameter]
    public TargetFileSelection? ToDisplay { get; set; }
}
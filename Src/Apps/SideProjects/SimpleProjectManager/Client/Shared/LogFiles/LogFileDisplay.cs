using System.Reactive.Linq;
using Microsoft.AspNetCore.Components;
using SimpleProjectManager.Client.ViewModels.LogFiles;
using Tauron.Application.Blazor;

namespace SimpleProjectManager.Client.Shared.LogFiles;

public sealed partial class LogFileDisplay
{
    [Parameter]
    public TargetFileSelection? ToDisplay { get; set; }

    protected override IEnumerable<IDisposable> InitializeModel()
    {
        if(ViewModel is null) yield break;
        
        yield return ViewModel.Changed
            .Where(c => string.Equals(c.PropertyName, nameof(LogFileDisplayViewModel.HasFile), StringComparison.Ordinal))
            .Subscribe(_ => RenderingManager.StateHasChanged());
    }
}
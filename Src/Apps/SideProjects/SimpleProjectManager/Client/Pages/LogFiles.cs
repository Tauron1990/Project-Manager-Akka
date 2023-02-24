using JetBrains.Annotations;
using ReactiveUI;
using Tauron.Application.Blazor.Commands;

namespace SimpleProjectManager.Client.Pages;

public sealed partial class LogFiles
{
    private MudCommandIconButton? _refreshButton;

    protected override IEnumerable<IDisposable> InitializeModel()
    {
        yield return this.BindCommand(ViewModel, m => m.RefreshLogs, v => v._refreshButton);
    }
}
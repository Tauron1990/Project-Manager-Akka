using Microsoft.AspNetCore.Components;
using ReactiveUI;
using SimpleProjectManager.Client.Shared.ViewModels.Devices;
using SimpleProjectManager.Shared.Services.Devices;
using Stl.Fusion;

namespace SimpleProjectManager.Client.Shared.Devices;

public sealed partial class DeviceDisplay
{
    private DevicePair? _toDisplay;

    [Parameter]
    public DevicePair? ToDisplay
    {
        get => _toDisplay;
        set => this.RaiseAndSetIfChanged(ref _toDisplay, value);
    }

    protected override IEnumerable<IDisposable> InitializeModel()
    {
        yield return this.Bind(ViewModel, m => m.DevicePair, v => v.ToDisplay);
    }
}
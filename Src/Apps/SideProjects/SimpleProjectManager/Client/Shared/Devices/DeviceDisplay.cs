using Microsoft.AspNetCore.Components;
using SimpleProjectManager.Client.Shared.ViewModels.Devices;

namespace SimpleProjectManager.Client.Shared.Devices;

public sealed partial class DeviceDisplay
{
    [Parameter]
    public DevicePair? ToDisplay { get; set; }
}
using Microsoft.AspNetCore.Components;
using SimpleProjectManager.Shared.Services.Devices;

namespace SimpleProjectManager.Client.Shared.Devices;

public sealed partial class DeviceUiDisplay
{
    [Parameter]
    public DeviceUiGroup? UiGroup { get; set; }
}
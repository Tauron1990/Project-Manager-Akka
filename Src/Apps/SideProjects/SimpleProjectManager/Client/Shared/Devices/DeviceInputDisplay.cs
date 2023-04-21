using Microsoft.AspNetCore.Components;
using SimpleProjectManager.Shared;
using SimpleProjectManager.Shared.Services.Devices;

namespace SimpleProjectManager.Client.Shared.Devices;

public partial class DeviceInputDisplay
{
    [Parameter]
    public DeviceId? DeviceId { get; set; }

    [Parameter]
    public DeviceId? Element { get; set; }

    [Parameter]
    public DisplayName Name { get; set; }

    [Parameter]
    public string Content { get; set; } = string.Empty;
}
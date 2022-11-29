using Microsoft.AspNetCore.Components;
using SimpleProjectManager.Shared.Services.Devices;

namespace SimpleProjectManager.Client.Shared.Devices;

public partial class SingleButtonDisplay
{
    [Parameter]
    public DeviceId? DeviceId { get; set; }

    [Parameter]
    public DeviceButton Button { get; set; }
}
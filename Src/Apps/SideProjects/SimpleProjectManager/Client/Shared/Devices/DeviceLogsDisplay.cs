using Microsoft.AspNetCore.Components;
using SimpleProjectManager.Shared.Services.Devices;

namespace SimpleProjectManager.Client.Shared.Devices;

public partial class DeviceLogsDisplay
{
    [Parameter]
    public DeviceId? DeviceId { get; set; }
}
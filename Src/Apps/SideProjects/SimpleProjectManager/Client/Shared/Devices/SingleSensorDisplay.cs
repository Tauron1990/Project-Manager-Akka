using Microsoft.AspNetCore.Components;
using SimpleProjectManager.Shared.Services.Devices;

namespace SimpleProjectManager.Client.Shared.Devices;

public partial class SingleSensorDisplay
{
    [Parameter]
    public DeviceSensor? Sensor { get; set; }

    [Parameter]
    public DeviceId? DeviceId { get; set; }
}
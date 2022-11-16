using Microsoft.AspNetCore.Mvc;
using SimpleProjectManager.Shared.ServerApi;
using SimpleProjectManager.Shared.Services.Devices;
using Stl.Fusion.Server;

namespace SimpleProjectManager.Server.Controllers;

[ApiController]
[JsonifyErrors]
[Route(ApiPaths.DeviceApi + "/[action]")]
public class DeviceController : Controller, IDeviceService
{
    private readonly IDeviceService _deviceService;

    public DeviceController(IDeviceService deviceService)
        => _deviceService = deviceService;

    [HttpGet]
    [Publish]
    public Task<DeviceList> GetAllDevices(CancellationToken token)
        => _deviceService.GetAllDevices(token);

    [HttpGet]
    [Publish]
    public Task<DeviceUiGroup> GetRootUi([FromQuery] DeviceId device, CancellationToken token)
        => _deviceService.GetRootUi(device, token);

    [HttpGet]
    [Publish]
    public Task<string> GetStringSensorValue([FromQuery] DeviceId device, [FromQuery] DeviceId sensor, CancellationToken token)
        => _deviceService.GetStringSensorValue(device, sensor, token);

    [HttpGet]
    [Publish]
    public Task<int> GetIntSensorValue([FromQuery] DeviceId device, [FromQuery] DeviceId sensor, CancellationToken token)
        => _deviceService.GetIntSensorValue(device, sensor, token);

    [HttpGet]
    [Publish]
    public Task<double> GetDoubleSensorValue([FromQuery] DeviceId device, [FromQuery] DeviceId sensor, CancellationToken token)
        => _deviceService.GetDoubleSensorValue(device, sensor, token);

    [HttpGet]
    [Publish]
    public Task<bool> CanClickButton([FromQuery] DeviceId device, [FromQuery] DeviceId button, CancellationToken token)
        => _deviceService.CanClickButton(device, button, token);

    [HttpGet]
    [Publish]
    public Task<DateTime> CurrentLogs(CancellationToken token)
        => _deviceService.CurrentLogs(token);

    [HttpGet]
    public Task<Logs> GetBatches([FromQuery] DeviceId deviceName, [FromQuery] DateTime from, CancellationToken token)
        => _deviceService.GetBatches(deviceName, from, token);

    [HttpPost]
    public async Task<SimpleResult> ClickButton([FromBody] DeviceId device, [FromQuery] DeviceId button, CancellationToken token)
    {
        try
        {
            return await _deviceService.ClickButton(device, button, token);
        }
        catch (Exception e)
        {
            return new SimpleResult(e.Message);
        }
    }
}
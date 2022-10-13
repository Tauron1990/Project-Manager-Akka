using Microsoft.AspNetCore.Mvc;
using SimpleProjectManager.Client.Operations.Shared.Devices;
using SimpleProjectManager.Shared.ServerApi;
using SimpleProjectManager.Shared.Services.Devices;
using Stl.Fusion.Server;

namespace SimpleProjectManager.Server.Controllers;

[ApiController, JsonifyErrors, Route(ApiPaths.DeviceApi + "/[action]")]
public class DeviceController : IDeviceService
{
    private readonly IDeviceService _deviceService;

    public DeviceController(IDeviceService deviceService)
        => _deviceService = deviceService;

    [HttpGet, Publish]
    public Task<string[]> GetAllDevices(CancellationToken token)
        => _deviceService.GetAllDevices(token);

    [HttpGet, Publish]
    public Task<DeviceUiGroup> GetRootUi([FromQuery]string device, CancellationToken token)
        => _deviceService.GetRootUi(device, token);

    [HttpGet, Publish]
    public Task<string> GetStringSensorValue([FromQuery]string device, [FromQuery]string sensor, CancellationToken token)
        => _deviceService.GetStringSensorValue(device, sensor, token);

    [HttpGet, Publish]
    public Task<int> GetIntSensorValue([FromQuery]string device, [FromQuery]string sensor, CancellationToken token)
        => _deviceService.GetIntSensorValue(device, sensor, token);

    [HttpGet, Publish]
    public Task<double> GetDoubleSensorValue([FromQuery]string device, [FromQuery]string sensor, CancellationToken token)
        => _deviceService.GetDoubleSensorValue(device, sensor, token);

    [HttpGet, Publish]
    public Task<bool> CanClickButton([FromQuery]string device, [FromQuery]string button, CancellationToken token)
        => _deviceService.CanClickButton(device, button, token);

    [HttpGet, Publish]
    public Task<DateTime> CurrentLogs(CancellationToken token)
        => _deviceService.CurrentLogs(token);

    [HttpGet]
    public Task<LogBatch[]> GetBatches([FromQuery]string deviceName, [FromQuery]DateTime from, CancellationToken token)
        => _deviceService.GetBatches(deviceName, from, token);

    [HttpPost]
    public async Task<string> ClickButton([FromBody]string device, [FromQuery]string button, CancellationToken token)
    {
        try
        {
            return await _deviceService.ClickButton(device, button, token);
        }
        catch (Exception e)
        {
            return e.Message;
        }
    }
}
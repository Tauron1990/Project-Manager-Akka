using System.Text.Json;
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
    public Task<DateTime> CurrentLogs([FromQuery(Name = "device")]DeviceId id, CancellationToken token)
        => _deviceService.CurrentLogs(id, token);

    [HttpGet]
    public Task<Logs> GetBatches(
        [FromQuery(Name = "deviceid")] DeviceId deviceId, 
        [FromQuery(Name = "from")] DateTime from, 
        [FromQuery(Name = "to")] DateTime to, 
        CancellationToken token)
        => _deviceService.GetBatches(deviceId, from, to, token);

    public Task<SimpleResult> ClickButton(DeviceId device, DeviceId button, CancellationToken token)
        => _deviceService.ClickButton(device, button, token);

    [HttpPost]
    public async Task<SimpleResult> ClickButton(CancellationToken token)
    {
        try
        {
            using JsonDocument body = await JsonDocument.ParseAsync(HttpContext.Request.Body, cancellationToken: token).ConfigureAwait(false);
            string? query = HttpContext.Request.Query["button"].Single();


            if(string.IsNullOrWhiteSpace(query))
                return SimpleResult.Failure("Keine Maschienen Id");

            var device = new DeviceId(body.RootElement.GetProperty("value").GetString() ?? string.Empty);
            var button = new DeviceId(query);

            return await ClickButton(device, button, token);
        }
        catch (Exception e)
        {
            return SimpleResult.Failure(e);
        }
    }
}
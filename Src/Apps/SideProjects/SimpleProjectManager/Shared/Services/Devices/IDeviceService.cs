using JetBrains.Annotations;
using SimpleProjectManager.Client.Operations.Shared.Devices;
using Stl.Fusion;

namespace SimpleProjectManager.Shared.Services.Devices;

[PublicAPI]
public interface IDeviceService
{
    [ComputeMethod]
    Task<string[]> GetAllDevices(CancellationToken token);

    [ComputeMethod]
    Task<DeviceUiGroup> GetRootUi(string device, CancellationToken token);

    [ComputeMethod]
    Task<string> GetStringSensorValue(string device, string sensor, CancellationToken token);
    
    [ComputeMethod]
    Task<int> GetIntSensorValue(string device, string sensor, CancellationToken token);
    
    [ComputeMethod]
    Task<double> GetDoubleSensorValue(string device, string sensor, CancellationToken token);

    [ComputeMethod]
    Task<bool> CanClickButton(string device, string button, CancellationToken token);

    [ComputeMethod]
    Task<DateTime> CurrentLogs(CancellationToken token);

    Task<LogBatch[]> GetBatches(DateTime from, CancellationToken token);

    Task ClickButton(string device, string button, CancellationToken token);
}
using JetBrains.Annotations;
using Stl.Fusion;
using Tauron.Operations;

namespace SimpleProjectManager.Shared.Services.Devices;

[PublicAPI]
public interface IDeviceService
{
    [ComputeMethod]
    Task<Devices> GetAllDevices(CancellationToken token);

    [ComputeMethod]
    Task<DeviceUiGroup> GetRootUi(DeviceId device, CancellationToken token);

    [ComputeMethod]
    Task<string> GetStringSensorValue(DeviceId device, DeviceId sensor, CancellationToken token);
    
    [ComputeMethod]
    Task<int> GetIntSensorValue(DeviceId device, DeviceId sensor, CancellationToken token);
    
    [ComputeMethod]
    Task<double> GetDoubleSensorValue(DeviceId device, DeviceId sensor, CancellationToken token);

    [ComputeMethod]
    Task<bool> CanClickButton(DeviceId device, DeviceId button, CancellationToken token);

    [ComputeMethod]
    Task<DateTime> CurrentLogs(CancellationToken token);

    Task<Logs> GetBatches(DeviceId deviceName, DateTime from, CancellationToken token);

    Task<SimpleResult> ClickButton(DeviceId device, DeviceId button, CancellationToken token);
}
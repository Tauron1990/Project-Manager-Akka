using JetBrains.Annotations;
using Stl.Fusion;
using Tauron.Operations;

namespace SimpleProjectManager.Shared.Services.Devices;

[PublicAPI]
public interface IDeviceService : IComputeService
{
    [ComputeMethod]
    Task<DeviceList> GetAllDevices(CancellationToken token);

    [ComputeMethod]
    Task<DeviceUiGroup> GetRootUi(Ic<DeviceId> device, CancellationToken token);

    [ComputeMethod]
    Task<string> GetStringSensorValue(Ic<DeviceId> device, Ic<DeviceId> sensor, CancellationToken token);

    [ComputeMethod]
    Task<int> GetIntSensorValue(Ic<DeviceId> device, Ic<DeviceId> sensor, CancellationToken token);

    [ComputeMethod]
    Task<double> GetDoubleSensorValue(Ic<DeviceId> device, Ic<DeviceId> sensor, CancellationToken token);

    [ComputeMethod]
    Task<bool> CanClickButton(Ic<DeviceId> device, Ic<DeviceId> button, CancellationToken token);

    [ComputeMethod]
    Task<DateTime> CurrentLogs(Ic<DeviceId> device, CancellationToken token);

    Task<Logs> GetBatches(Ic<DeviceId> deviceId, DateTime from, DateTime to, CancellationToken token);

    Task<SimpleResultContainer> ClickButton(Ic<DeviceId> device, Ic<DeviceId> button, CancellationToken token);

    Task<SimpleResultContainer> DeviceInput(DeviceInputData inputData, CancellationToken token);
}
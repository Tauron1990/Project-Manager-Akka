using JetBrains.Annotations;
using RestEase;
using SimpleProjectManager.Client.Operations.Shared.Devices;

namespace SimpleProjectManager.Shared.ServerApi.RestApi;

[BasePath(ApiPaths.DeviceApi), UsedImplicitly(ImplicitUseTargetFlags.Members)]
public interface IDeviceServiceDef
{
    [Get(nameof(GetAllDevices))]
    Task<string[]> GetAllDevices(CancellationToken token);

    [Get(nameof(GetRootUi))]
    Task<DeviceUiGroup> GetRootUi([Query] string device, CancellationToken token);

    [Get(nameof(GetStringSensorValue))]
    Task<string> GetStringSensorValue([Query] string device, [Query] string sensor, CancellationToken token);

    [Get(nameof(GetIntSensorValue))]
    Task<int> GetIntSensorValue([Query]string device, [Query]string sensor, CancellationToken token);
    
    [Get(nameof(GetDoubleSensorValue))]
    Task<double> GetDoubleSensorValue([Query]string device, [Query]string sensor, CancellationToken token);

    [Get(nameof(CanClickButton))]
    Task<bool> CanClickButton([Query]string device, [Query]string button, CancellationToken token);

    [Get(nameof(CurrentLogs))]
    Task<DateTime> CurrentLogs(CancellationToken token);

    [Get(nameof(GetBatches))]
    Task<LogBatch[]> GetBatches([Query]string deviceName, [Query]DateTime from, CancellationToken token);

    [Post(nameof(ClickButton))]
    Task<string> ClickButton([Body]string device, [Query]string button, CancellationToken token);
}
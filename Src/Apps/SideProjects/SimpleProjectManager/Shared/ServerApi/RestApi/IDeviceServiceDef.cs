using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using RestEase;
using SimpleProjectManager.Shared.Services.Devices;
using Tauron.Operations;

namespace SimpleProjectManager.Shared.ServerApi.RestApi;

[BasePath(ApiPaths.DeviceApi)]
[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
public interface IDeviceServiceDef
{
    [Get(nameof(GetAllDevices))]
    Task<DeviceList> GetAllDevices(CancellationToken token);

    [Get(nameof(GetRootUi))]
    Task<DeviceUiGroup> GetRootUi([Query] DeviceId device, CancellationToken token);

    [Get(nameof(GetStringSensorValue))]
    Task<string> GetStringSensorValue([Query] DeviceId device, [Query] DeviceId sensor, CancellationToken token);

    [Get(nameof(GetIntSensorValue))]
    Task<int> GetIntSensorValue([Query] DeviceId device, [Query] DeviceId sensor, CancellationToken token);

    [Get(nameof(GetDoubleSensorValue))]
    Task<double> GetDoubleSensorValue([Query] DeviceId device, [Query] DeviceId sensor, CancellationToken token);

    [Get(nameof(CanClickButton))]
    Task<bool> CanClickButton([Query] DeviceId device, [Query] DeviceId button, CancellationToken token);

    [Get(nameof(CurrentLogs))]
    Task<DateTime> CurrentLogs(CancellationToken token);

    [Get(nameof(GetBatches))]
    Task<Logs> GetBatches([Query] DeviceId deviceName, [Query] DateTime from, CancellationToken token);

    [Post(nameof(ClickButton))]
    Task<SimpleResult> ClickButton([Body] DeviceId device, [Query("button")] DeviceId button, CancellationToken token);
}
using System.Globalization;
using SimpleProjectManager.Client.Shared.Devices;
using SimpleProjectManager.Shared.Services.Devices;
using Stl.Fusion;
using Tauron.Application.Blazor;

namespace SimpleProjectManager.Client.ViewModels.Devices;

public sealed class SingleSensorViewModel : BlazorViewModel
{
    private readonly IDeviceService _deviceService;
    private readonly IState<DeviceSensor?> _sensor;
    private readonly IState<DeviceId?> _deviceId;

    public IState<string> SensorValue { get; }
    
    public SingleSensorViewModel(IStateFactory stateFactory, IDeviceService deviceService)
        : base(stateFactory)
    {
        _deviceService = deviceService;
        _sensor = GetParameter<DeviceSensor?>(nameof(SingleSensorDisplay.Sensor));
        _deviceId = GetParameter<DeviceId?>(nameof(SingleSensorDisplay.DeviceId));

        SensorValue = stateFactory.NewComputed(new ComputedState<string>.Options(), GetSensorValue);
    }

    private async Task<string> GetSensorValue(IComputedState<string> old, CancellationToken token)
    {
        DeviceId? device = await _deviceId.Use(token).ConfigureAwait(false);
        DeviceSensor? sensor = await _sensor.Use(token).ConfigureAwait(false);

        if(device is null || sensor is null) return "Keine Daten";

        switch (sensor.SensorType)
        {

            case SensorType.None:
                return "Kein Type angegeben";
            case SensorType.Double:
                double doubleData = await _deviceService.GetDoubleSensorValue(device, sensor.Identifer, token).ConfigureAwait(false);

                return doubleData.ToString("#.000", CultureInfo.CurrentUICulture);
            case SensorType.String:
                return await _deviceService.GetStringSensorValue(device, sensor.Identifer, token).ConfigureAwait(false);
            case SensorType.Number:
                int intData = await _deviceService.GetIntSensorValue(device, sensor.Identifer, token).ConfigureAwait(false);

                return intData.ToString(CultureInfo.CurrentUICulture);
            default:
                return "Sensor Type Fehlerhaft";
        }
    }
}
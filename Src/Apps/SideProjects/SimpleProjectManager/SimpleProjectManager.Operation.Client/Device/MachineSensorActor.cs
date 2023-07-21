using Akka.Actor;
using Microsoft.Extensions.Logging;
using SimpleProjectManager.Operation.Client.Device.Core;
using SimpleProjectManager.Shared.Services.Devices;
using Tauron;
using static SimpleProjectManager.Client.Operations.Shared.Devices.DeviceManagerMessages;

namespace SimpleProjectManager.Operation.Client.Device;

public sealed partial class MachineSensorActor : ReceiveActor, IWithTimers
{
    private readonly IActorRef _deviceDeviceManager;
    private readonly DeviceId _deviceName;
    private readonly ILogger<MachineSensorActor> _logger;
    private readonly IMachine _machine;
    private readonly DeviceSensor _target;

    private ISensorBox _sensorBox;

    public MachineSensorActor(DeviceId deviceName, IMachine machine, DeviceSensor target, IActorRef deviceDeviceManager, ILogger<MachineSensorActor> logger)
    {
        _deviceName = deviceName;
        _machine = machine;
        _target = target;
        _deviceDeviceManager = deviceDeviceManager;
        _logger = logger;
        _sensorBox = SensorBox.CreateDefault(target.SensorType);

        Receive<RunUpdate>(OnUpdate);
        Receive<Status.Failure>(f => ErrorOnRunUpdate(f.Cause, target));
        Receive<ISensorBox>(ValueRecived);
        
        Receive<DeviceServerOffline>(_ => {});
        Receive<DeviceServerOnline>(_ => { });
    }

    public ITimerScheduler Timers { get; set; } = null!;

    private void ValueRecived(ISensorBox obj)
    {
        if(_sensorBox.Equals(obj))
        {
            SheduleUpdate();
            return;
        }

        _sensorBox = obj;
        _deviceDeviceManager.Tell(new UpdateSensor(_deviceName, _target.Identifer, _sensorBox));

        SheduleUpdate();
    }

    [LoggerMessage(EventId = 67, Level = LogLevel.Error, Message = "Error on Update Sensor {sensor}")]
    private partial void ErrorOnRunUpdate(Exception exception, DeviceSensor sensor);

    private void OnUpdate(RunUpdate pam)
        => _machine.UpdateSensorValue(_target).PipeTo(Self).Ignore();

    protected override void PreStart()
    {
        SheduleUpdate();
        base.PreStart();
    }

    private void SheduleUpdate()
        => Timers.StartSingleTimer(nameof(SheduleUpdate), RunUpdate.Inst, TimeSpan.FromSeconds(1));
}
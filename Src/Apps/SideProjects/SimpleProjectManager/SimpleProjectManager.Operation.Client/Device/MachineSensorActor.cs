using Akka.Actor;
using Microsoft.Extensions.Logging;
using SimpleProjectManager.Client.Operations.Shared.Devices;

using static  SimpleProjectManager.Client.Operations.Shared.Devices.DeviceManagerMessages;

namespace SimpleProjectManager.Operation.Client.Device;

public sealed partial class MachineSensorActor : ReceiveActor, IWithTimers
{
    private sealed record RunUpdate
    {
        public static readonly RunUpdate Inst = new();
    }
    
    private readonly IMachine _machine;
    private readonly DeviceSensor _target;
    private readonly IActorRef _deviceDeviceManager;
    private readonly ILogger<MachineSensorActor> _logger;

    private ISensorBox _sensorBox;

    public MachineSensorActor(IMachine machine, DeviceSensor target, IActorRef deviceDeviceManager, ILogger<MachineSensorActor> logger)
    {
        _machine = machine;
        _target = target;
        _deviceDeviceManager = deviceDeviceManager;
        _logger = logger;
        _sensorBox = SensorBox.CreateDefault(target.SensorType);

        Receive<RunUpdate>(OnUpdate);
        Receive<Status.Failure>(f => ErrorOnRunUpdate(f.Cause, _target));
        Receive<ISensorBox>(ValueRecived);
    }

    private void ValueRecived(ISensorBox obj)
    {
        
    }

    [LoggerMessage(EventId = 67, Level = LogLevel.Error, Message = "Error on Update Sensor {sensor}")]
    private partial void ErrorOnRunUpdate(Exception exception, DeviceSensor sensor);
    
    private void OnUpdate(RunUpdate pam)
        => _machine.UpdateSensorValue(_target).PipeTo(Self);

    protected override void PreStart()
    {
        SheduleUpdate();
        base.PreStart();
    }
    
    private void SheduleUpdate()
        => Timers.StartSingleTimer(nameof(SheduleUpdate), RunUpdate.Inst, TimeSpan.FromSeconds(1));

    public ITimerScheduler Timers { get; set; } = null!;
}
using Akka.Actor;
using SimpleProjectManager.Client.Operations.Shared.Devices;
using SimpleProjectManager.Operation.Client.Device.Core;
using SimpleProjectManager.Shared.Services.Devices;

namespace SimpleProjectManager.Operation.Client.Device;

public sealed class LoggerActor : ReceiveActor, IWithTimers
{
    private readonly DeviceId _deviceName;
    private readonly LogDistribution _logDistribution;
    private readonly IMachine _machine;

    private bool _isOnline;

    public LoggerActor(IMachine machine, DeviceId deviceName)
    {
        _machine = machine;
        _deviceName = deviceName;
        _logDistribution = new LogDistribution(Context.System);

        ReceiveAsync<RunUpdate>(OnSendLog);
        Receive<DeviceServerOnline>(_ =>
                                    {
                                        _isOnline = true;
                                    });
        Receive<DeviceServerOffline>(_ =>
                                     {
                                         _isOnline = false;
                                     });
    }

    public ITimerScheduler Timers { get; set; } = null!;

    protected override void PreStart()
    {
        SheduleUpdate();
        base.PreStart();
    }

    private void SheduleUpdate()
        => Timers.StartSingleTimer(nameof(OnSendLog), RunUpdate.Inst, TimeSpan.FromSeconds(5));

    private async Task OnSendLog(RunUpdate _)
    {
        try
        {
            if(!_isOnline) return;

            LogBatch batch = await _machine.NextLogBatch().ConfigureAwait(false);

            if(batch.IsEmpty) return;

            _logDistribution.Publish(batch with { DeviceName = _deviceName });

        }
        finally
        {
            SheduleUpdate();
        }
    }
}
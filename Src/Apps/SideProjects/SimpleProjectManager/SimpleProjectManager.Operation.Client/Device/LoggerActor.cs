using Akka.Actor;
using SimpleProjectManager.Client.Operations.Shared.Devices;
using SimpleProjectManager.Shared.Services.Devices;

namespace SimpleProjectManager.Operation.Client.Device;

public sealed class LoggerActor : ReceiveActor, IWithTimers
{
    private readonly DeviceId _deviceName;
    private readonly LogDistribution _logDistribution;
    private readonly IMachine _machine;

    public LoggerActor(IMachine machine, DeviceId deviceName)
    {
        _machine = machine;
        _deviceName = deviceName;
        _logDistribution = new LogDistribution(Context.System);

        ReceiveAsync<RunUpdate>(OnSendLog);
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
        LogBatch batch = await _machine.NextLogBatch().ConfigureAwait(false);

        if(batch.IsEmpty) return;

        _logDistribution.Publish(batch with { DeviceName = _deviceName });

        SheduleUpdate();
    }
}
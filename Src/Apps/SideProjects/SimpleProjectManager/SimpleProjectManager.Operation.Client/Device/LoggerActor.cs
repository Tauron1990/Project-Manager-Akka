using Akka.Actor;
using SimpleProjectManager.Client.Operations.Shared.Devices;

namespace SimpleProjectManager.Operation.Client.Device;

public sealed class LoggerActor : ReceiveActor, IWithTimers
{
    private readonly IMachine _machine;
    private readonly string _deviceName;
    private readonly LogDistribution _logDistribution;
    
    public ITimerScheduler Timers { get; set; } = null!;
    
    public LoggerActor(IMachine machine, string deviceName)
    {
        _machine = machine;
        _deviceName = deviceName;
        _logDistribution = new LogDistribution(Context.System);
        
        ReceiveAsync<RunUpdate>(OnSendLog);
    }

    protected override void PreStart()
    {
        SheduleUpdate();
        base.PreStart();
    }

    private void SheduleUpdate()
        => Timers.StartSingleTimer(nameof(OnSendLog), RunUpdate.Inst, TimeSpan.FromSeconds(5));
    
    private async Task OnSendLog(RunUpdate _)
    {
        var batch = await _machine.NextLogBatch();
        if(batch.Logs.IsEmpty) return;
        
        _logDistribution.Publish(batch with{ DeviceName = _deviceName });
        
        SheduleUpdate();
    }
}
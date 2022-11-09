using System.Collections.Immutable;

namespace SimpleProjectManager.Shared.Services.Devices;

public sealed record LogBatch(DeviceId DeviceName, ImmutableList<LogData> Logs)
{
    private static DeviceId _dummyId = DeviceId.New;
    
    public LogBatch(ImmutableList<LogData> logDatas)
        : this(_dummyId, logDatas){}
    
}
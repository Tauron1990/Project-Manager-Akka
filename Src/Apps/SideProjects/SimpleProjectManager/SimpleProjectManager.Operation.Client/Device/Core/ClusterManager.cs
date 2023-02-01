using Akka.Actor;
using Akka.Cluster;
using Akka.DistributedData;
using Microsoft.Extensions.Logging;
using SimpleProjectManager.Client.Operations.Shared;
using SimpleProjectManager.Client.Operations.Shared.Devices;
using SimpleProjectManager.Shared.Services.Devices;
using Tauron;

namespace SimpleProjectManager.Operation.Client.Device.Core;

public sealed partial class ClusterManager : ReceiveActor
{
    private readonly ILogger<ClusterManager> _logger = LoggingProvider.LoggerFactory.CreateLogger<ClusterManager>();
    private readonly DistributedData _replicator;
    private readonly ORDictionaryKey<string, DeviceInfoEntry> _dictionaryKey;
    private readonly Cluster _cluster;
    
    public ClusterManager()
    {
        _replicator = Context.System.DistributedData();
        _cluster = Cluster.Get(Context.System);
        
        _dictionaryKey = new ORDictionaryKey<string, DeviceInfoEntry>(DeviceManagerMessages.DeviceDataId);

        Receive<DeviceInformations>(NewDeviceData);
        Receive<Status.Failure>(
            fail =>
            {
                FatalErrorOnUpdate(fail.Cause);
                Context.Stop(Context.Self);
            });
    }

    private void NewDeviceData(DeviceInformations obj)
    {
        RunUpdate(obj)
           .PipeTo(Self)
           .Ignore();
    }

    private async Task RunUpdate(DeviceInformations info)
    {
        try
        {
            UpdateDeviceInfo(info.DeviceId, info.Name);

            var dic = await _replicator.GetAsync(_dictionaryKey).ConfigureAwait(false)
                   ?? ORDictionary<string, DeviceInfoEntry>.Empty;

            var newEntry = DeviceInfoEntry.Create(info.DeviceManager, info);

            dic = dic.AddOrUpdate(
                _cluster,
                info.DeviceId.Value,
                newEntry,
                entry => entry.With(info));

            await _replicator.UpdateAsync(_dictionaryKey, dic, new WriteTo(2, TimeSpan.FromSeconds(20))).ConfigureAwait(false);
        }
        catch (Exception exception)
        {
            if(exception is not TimeoutException)
                throw;

            ReplicatorUpdateTimeout(info.DeviceId, info.Name);
        }
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "Timeout while Updating {name} with id {id}")]
    private partial void ReplicatorUpdateTimeout(DeviceId id, in DeviceName name);
    
    [LoggerMessage(Level = LogLevel.Information, Message = "New Data for Device {name} with id {id}")]
    private partial void UpdateDeviceInfo(DeviceId id, in DeviceName name);
    
    [LoggerMessage(Level = LogLevel.Critical, Message = "Critical error on Update Device Data")]
    private partial void FatalErrorOnUpdate(Exception ex);
}
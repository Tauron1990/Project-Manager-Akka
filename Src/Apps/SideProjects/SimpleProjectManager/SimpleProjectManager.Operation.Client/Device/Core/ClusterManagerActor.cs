using Akka.Actor;
using Akka.Cluster;
using Akka.DistributedData;
using Microsoft.Extensions.Logging;
using SimpleProjectManager.Client.Operations.Shared.Devices;
using SimpleProjectManager.Shared.Services.Devices;
using Tauron;
using Tauron.Application;

#pragma warning disable EPS02
#pragma warning disable SYSLIB1006

namespace SimpleProjectManager.Operation.Client.Device.Core;

public sealed partial class ClusterManagerActor : ReceiveActor
{
    private readonly ILogger<ClusterManagerActor> _logger = TauronEnviroment.LoggerFactory.CreateLogger<ClusterManagerActor>();
    private readonly DistributedData _replicator;
    private readonly ORDictionaryKey<string, DeviceInfoEntry> _dictionaryKey;
    private readonly Cluster _cluster;
    
    public ClusterManagerActor()
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
        
        Receive<DeviceServerOffline>(_ => {});
        Receive<DeviceServerOnline>(_ => { });
    }

    private void NewDeviceData(DeviceInformations obj)
    {
        RunUpdate(obj, Self)
           .PipeTo(Self)
           .Ignore();
    }

    private async Task RunUpdate(DeviceInformations info, IActorRef client)
    {
        try
        {
            UpdateDeviceInfo(info.DeviceId, info.Name);

            var dic = await _replicator.GetAsync(_dictionaryKey).ConfigureAwait(false)
                   ?? ORDictionary<string, DeviceInfoEntry>.Empty;

            var newEntry = DeviceInfoEntry.Create(client, info);

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
            #pragma warning disable ERP022
        }
        #pragma warning restore ERP022
    }

    [LoggerMessage(EventId = 1, Level = LogLevel.Warning, Message = "Timeout while Updating {name} with id {id}")]
    private partial void ReplicatorUpdateTimeout(DeviceId id, in DeviceName name);
    
    [LoggerMessage(EventId = 2, Level = LogLevel.Information, Message = "New Data for Device {name} with id {id}")]
    private partial void UpdateDeviceInfo(DeviceId id, in DeviceName name);
    
    [LoggerMessage(Level = LogLevel.Critical, Message = "Critical error on Update Device Data")]
    private partial void FatalErrorOnUpdate(Exception ex);
}
using Akka.Actor;
using Akka.Cluster;
using Akka.DistributedData;
using SimpleProjectManager.Client.Operations.Shared;
using SimpleProjectManager.Client.Operations.Shared.Devices;

namespace SimpleProjectManager.Server.Core.DeviceManager;

public sealed partial class DeviceClusterManager : ReceiveActor
{
    private readonly ILogger<DeviceClusterManager> _logger = LoggingProvider.LoggerFactory.CreateLogger<DeviceClusterManager>();
    private readonly ORDictionaryKey<string, DeviceInfoEntry> _dictionaryKey;
    private readonly DistributedData _replicator;
    private readonly Cluster _cluster;
    
    private readonly Dictionary<string, DeviceInfoEntry> _infoEntries = new(StringComparer.Ordinal);
    private readonly Dictionary<string, DeviceInfoEntry> _buffer = new(StringComparer.Ordinal);

    private readonly Dictionary<IActorRef, string> _deathPactEntries = new();

    public DeviceClusterManager()
    {
        _replicator = Context.System.DistributedData();
        _cluster = Cluster.Get(Context.System);
        _dictionaryKey = new ORDictionaryKey<string, DeviceInfoEntry>(DeviceManagerMessages.DeviceDataId);
        
        _replicator.Replicator.Tell(Dsl.Subscribe(_dictionaryKey, Self));
        _replicator.GetAsync(_dictionaryKey, ReadLocal.Instance)
           .PipeTo(Self)
           .Ignore();
        
        Receive<Status.Failure>(OnError);
        Receive<ORDictionary<string, DeviceInfoEntry>>(CurrentDevices);
        Receive<Changed>(ChangedData);
        Receive<Terminated>(ClientTerminated);
    }

    private void ApplyDictonaryChanges(ORDictionary<string, DeviceInfoEntry> dic)
    {
        
    }
    
    private void CurrentDevices(ORDictionary<string, DeviceInfoEntry> dictionary)
        => ApplyDictonaryChanges(dictionary);

    private void ChangedData(Changed obj)
        => ApplyDictonaryChanges(obj.Get(_dictionaryKey));

    [LoggerMessage(Level = LogLevel.Warning, Message = "Unkowen Client Actor {path}")]
    private partial void UnkowenClientActor(ActorPath path);
    
    private void ClientTerminated(Terminated obj)
    {
        if(_deathPactEntries.Remove(obj.ActorRef, out string? id))
        {
            RemoveEntry(id).PipeTo(Self).Ignore();
            return;
        }
        
        UnkowenClientActor(obj.ActorRef.Path);
    }

    private async Task RemoveEntry(string id)
    {
        var data = await _replicator.GetAsync(_dictionaryKey).ConfigureAwait(false);
        if(data is null)
            return;

        await _replicator.UpdateAsync(_dictionaryKey, data.Remove(_cluster, id), new WriteTo(1, TimeSpan.FromSeconds(20))).ConfigureAwait(false);
    }
    
    [LoggerMessage(Level = LogLevel.Error, Message = "Error on Replicate Data for Server")]
    private partial void ReplicatorError(Exception ex);
    
    private void OnError(Status.Failure obj)
        => ReplicatorError(obj.Cause);
}
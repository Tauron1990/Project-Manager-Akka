using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Cluster.Utility;
using Microsoft.Extensions.Logging;
using SimpleProjectManager.Client.Operations.Shared.Clustering;
using SimpleProjectManager.Client.Shared.Data.States;
using SimpleProjectManager.Shared.Services.LogFiles;

namespace SimpleProjectManager.Server.Core.Clustering;

public sealed partial class ClusterLogManager : ReceiveActor
{
    private readonly ILogger<ClusterLogManager> _logger;
    private readonly List<IActorRef> _logProviders = new();
    private readonly FileTracker _tracker = new();

    public ClusterLogManager(ILogger<ClusterLogManager> logger)
    {
        _logger = logger;
        
        Receive<ClusterActorDiscoveryMessage.ActorUp>(up => _logProviders.Add(up.Actor));
        Receive<ClusterActorDiscoveryMessage.ActorDown>(down => _logProviders.Remove(down.Actor));

        Receive<QueryLogFileNames>(_ => QueryNames().PipeTo(Sender));
        Receive<LogFileRequest>(rq => QueryContent(rq).PipeTo(Sender));
    }

    [LoggerMessage(Level = LogLevel.Error, EventId = 1, Message = "Error On Query Names from hosts")]
    private partial void ErrorOnBuildLogFileNameList(Exception e);

    [LoggerMessage(Level = LogLevel.Warning, EventId = 2, Message = "Query Names from Host was Canceled")]
    private partial void QueryNamesCanceled(Exception e);
    
    private async Task<LogFilesData> QueryNames()
    {
        var actors = _logProviders.ToArray();
        _tracker.Clear();
        
        try
        {
            var result = await Task.WhenAll(actors.Select(RunQuery)).ConfigureAwait(false);

            foreach ((LogFilesNamesResponse response, IActorRef from) in result)
                _tracker.Add(response.Name, from, response.Files);

            return _tracker.GetData();
        }
        catch (Exception e)
        {
            if(e is OperationCanceledException canceledException)
                QueryNamesCanceled(canceledException);
            else
                ErrorOnBuildLogFileNameList(e);
            
            return new LogFilesData(ImmutableList<LogFileEntry>.Empty);
        }
        
        async Task<(LogFilesNamesResponse, IActorRef)> RunQuery(IActorRef to)
        {
                LogFilesNamesResponse? result = await to.Ask<LogFilesNamesResponse>(new QueryLogFileNames(), TimeSpan.FromSeconds(10)).ConfigureAwait(false);
                return (result, to);
        }
    }

    [LoggerMessage(Level = LogLevel.Error, EventId = 3, Message = "Error On Query Log File {fileName} from host {hostName}")]
    private partial void ErrorOnRequestContent(Exception e, string hostName, string fileName);
    
    private async Task<LogFileContent> QueryContent(LogFileRequest request)
    {
        try
        {
            LogFileResponse result = await _tracker.QueryContent(request.Host, request.Name).ConfigureAwait(false);

            return new LogFileContent(result.Log);
        }
        catch (Exception e)
        {
            ErrorOnRequestContent(e, request.Host, request.Name);
            return new LogFileContent(string.Empty);
        }
    }

    protected override void PreStart()
    {
        ClusteringApi.Get(Context.System).Subscribe();
        base.PreStart();
    }
}
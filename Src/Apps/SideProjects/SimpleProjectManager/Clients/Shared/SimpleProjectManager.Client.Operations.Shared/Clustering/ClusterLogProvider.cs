using System.Collections.Immutable;
using Akka.Actor;
using Microsoft.Extensions.Logging;
using Tauron;

namespace SimpleProjectManager.Client.Operations.Shared.Clustering;

public sealed partial class ClusterLogProvider : ReceiveActor
{
    private readonly ILogger<ClusterLogProvider> _logger;

    public ClusterLogProvider(ILogger<ClusterLogProvider> logger)
    {
        _logger = logger;
        
        Receive<QueryLogFileNames>(QueryFiles);
        Receive<QueryLogFile>(
            msg => QueryFile(msg).PipeTo(
                Sender,
                Self,
                failure: e =>
                {
                    ErrorQueryFile(e);
                    return new LogFileResponse(string.Empty);
                }).Ignore());
    }

    [LoggerMessage(Level = LogLevel.Error, EventId = 1, Message = "Error on Query Log Feil")]
    private partial void ErrorQueryFile(Exception e);
    
    private async Task<LogFileResponse> QueryFile(QueryLogFile query)
    {
        try
        {
            string path = Path.GetFullPath("Logs");
            path = Path.Combine(path, query.Name);
            
            return new LogFileResponse(await File.ReadAllTextAsync(path).ConfigureAwait(false));
        }
        catch (Exception e)
        {
            ErrorQueryFile(e);
            return new LogFileResponse(string.Empty);
        }
    }

    [LoggerMessage(EventId = 0, Level = LogLevel.Error, Message = "Error on Query Log Files")]
    private partial void ErrorQueryFiles(Exception e);
    
    private void QueryFiles(QueryLogFileNames _)
    {
        try
        {
            string path = Path.GetFullPath("Logs");

            if(Directory.Exists(path))
            {
                string[] files = Directory.GetFiles(path, "*.log");
                Sender.Tell(new LogFilesNamesResponse(ImmutableList<string>.Empty.AddRange(files)));
            }
            else
                Sender.Tell(new LogFilesNamesResponse(ImmutableList<string>.Empty));
        }
        catch (Exception e)
        {
            ErrorQueryFiles(e);
            Sender.Tell(new LogFilesNamesResponse(ImmutableList<string>.Empty));
        }
    }
}
using System.Collections.Immutable;
using Akka.Actor;
using Microsoft.Extensions.Logging;
using Tauron;

namespace SimpleProjectManager.Client.Operations.Shared.Clustering;

public sealed partial class ClusterLogProvider : ReceiveActor
{
    private readonly string _name;
    private readonly ILogger<ClusterLogProvider> _logger;

    public ClusterLogProvider(string name, ILogger<ClusterLogProvider> logger)
    {
        _name = name;
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

#pragma warning disable MA0004
            await using Stream file = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using var data = new MemoryStream();
#pragma warning restore MA0004

            await file.CopyToAsync(data).ConfigureAwait(false);

            return !data.TryGetBuffer(buffer: out var buffer) 
                ? new LogFileResponse(string.Empty) 
                : new LogFileResponse(Convert.ToBase64String(buffer));

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
                IEnumerable<string> files =
                    from file in Directory.EnumerateFiles(path, "*.log")
                    let name = Path.GetFileName(file)
                    where !string.IsNullOrWhiteSpace(name)
                    select name;
                
                Sender.Tell(new LogFilesNamesResponse(_name, ImmutableList<string>.Empty.AddRange(files)));
            }
            else
                Sender.Tell(new LogFilesNamesResponse(_name, ImmutableList<string>.Empty));
        }
        catch (Exception e)
        {
            ErrorQueryFiles(e);
            Sender.Tell(new LogFilesNamesResponse(_name, ImmutableList<string>.Empty));
        }
    }
}
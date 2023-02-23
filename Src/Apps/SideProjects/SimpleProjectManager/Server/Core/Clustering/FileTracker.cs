using System.Collections.Concurrent;
using System.Collections.Immutable;
using Akka.Actor;
using SimpleProjectManager.Client.Operations.Shared.Clustering;
using SimpleProjectManager.Shared.Services.LogFiles;

namespace SimpleProjectManager.Server.Core.Clustering;

internal sealed class FileTracker
{
    private sealed record Entry(IActorRef From, ImmutableList<string> Files);

    private readonly ConcurrentDictionary<string, Entry> _entries = new(StringComparer.Ordinal);

    internal void Add(string host, IActorRef from, ImmutableList<string> files)
    {
        var entry = new Entry(from, files);
        _entries.AddOrUpdate(host, static (_, val) => val, static (_, _, val) => val, entry);
    }

    internal async ValueTask<LogFileResponse> QueryContent(string host, string name)
    {
        if(_entries.TryGetValue(host, out var entry))
        {
            if(entry.Files.Contains(name, StringComparer.Ordinal))
                return await entry.From.Ask<LogFileResponse>(new QueryLogFile(name), TimeSpan.FromSeconds(20)).ConfigureAwait(false);
        }

        throw new InvalidOperationException("Host not Found");
    }
    
    internal void Clear()
        => _entries.Clear();

    internal LogFilesData GetData() => new(_entries.Select(p => new LogFileEntry(p.Key, p.Value.Files)).ToImmutableList());
}
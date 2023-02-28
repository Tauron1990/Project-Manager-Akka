using System.Collections.Immutable;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Akka.Actor;
using Akka.Cluster.Utility;
using Microsoft.Extensions.Logging;
using SimpleProjectManager.Shared;
using Tauron;
using Tauron.Application.AkkaNode.Services.FileTransfer;
using Tauron.Features;
using Tauron.ObservableExt;

namespace SimpleProjectManager.Operation.Client.ImageEditor;

public sealed partial class ImageManagerFeature : ActorFeatureBase<ImageManagerFeature.State>
{
    private ILogger _logger = null!;

    public static IPreparedFeature Create(string targetPath, ImageManagerMode mode)
        => Feature.Create(
            () => new ImageManagerFeature(),
            c => new State(
                targetPath,
                string.Empty,
                mode,
                DataTransferManager.Empty,
                DataTransferManager.New(c, "FileTransfer"),
                ImmutableDictionary<ProjectFileId, string>.Empty,
                new FileSystemWatcher(targetPath),
                Disposable.Empty,
                new Subject<bool>()));

    protected override void ConfigImpl()
    {
        _logger = base.Logger;

        CurrentState.Watcher.EnableRaisingEvents = true;
        CurrentState.Watcher.DisposeWith(this);

        ClusterActorDiscovery.Get(Context.System).RegisterActor(new ClusterActorDiscoveryMessage.RegisterActor(Self, nameof(ImageManagerFeature)));

        Observ<RegisterServerManager>(
            obs => from msg in obs.Do(m => m.Sender.Tell(new ServerManagerResponse(m.State.Self)))
                   select msg.State with { Server = msg.Event.ServerManager });

        Observ<StartProject>(
            obs => obs.ConditionalSelect()
               .ToResult<State>(
                    b => b
                       .When(s => s.State.Mode == ImageManagerMode.Single, o => o.Select(StartSingle))
                       .When(s => s.State.Mode == ImageManagerMode.Multiple, o => o.Select(StartMultiple))));
    }

    [LoggerMessage(EventId = 38, Level = LogLevel.Error, Message = "Error on Start Project {jobName} with multiple mode.")]
    private partial void LogStartMultipleProject(Exception ex, string jobName);

    private State StartMultiple(StatePair<StartProject, State> obj)
    {
        (StartProject evt, State state) = obj;

        try
        {
            string jobName = evt.JobName;
            string targetPath = Path.Combine(state.CurrentPath, jobName);

            State newState = UpdateFileSystemWatcher(state, targetPath);
            CreateOrClearDirectory(targetPath);
            obj.Sender.Tell(new ProjectStartResponse(null));

            return newState with { CurrentPath = targetPath };
        }
        catch (Exception e)
        {
            LogStartMultipleProject(e, evt.JobName);
            obj.Sender.Tell(new ProjectStartResponse(e.Message));
        }

        return obj.State with { CurrentPath = string.Empty };
    }

    [LoggerMessage(EventId = 39, Level = LogLevel.Error, Message = "Error on Start Project {jobName} with Single Mode")]
    private partial void LogStartSingle(Exception ex, string jobName);

    private State StartSingle(StatePair<StartProject, State> obj)
    {
        string jobName = obj.Event.JobName;

        try
        {
            CreateOrClearDirectory(obj.State.TargetPath);
            State state = UpdateFileSystemWatcher(obj.State, obj.State.TargetPath);

            obj.Sender.Tell(new ProjectStartResponse(null));

            return state with { CurrentPath = obj.State.TargetPath };
        }
        catch (Exception e)
        {
            LogStartSingle(e, jobName);
            obj.Sender.Tell(new ProjectStartResponse(e.Message));
        }

        return obj.State with { CurrentPath = string.Empty };
    }

    private State UpdateFileSystemWatcher(State currentState, string newPath)
    {
        FileSystemWatcher watcher = currentState.Watcher;
        ((IResourceHolder)this).RemoveResource(currentState.Subscription);

        IActorRef self = Self;
        int newToken = currentState.Token + 1;
        watcher.Path = newPath;

        var rename = PrepareEventStream(
                currentState.DispatchFileSystemEvents,
                Observable.FromEvent<RenamedEventHandler, RenamedEventArgs>(
                    a => (_, args) => a(args),
                    d => watcher.Renamed += d,
                    d => watcher.Renamed -= d))
           .Select(s => HandleRename(s, newToken));

        var changed = PrepareEventStream(
                currentState.DispatchFileSystemEvents,
                Observable.FromEvent<FileSystemEventHandler, FileSystemEventArgs>(
                    a => (_, args) => a(args),
                    d => watcher.Changed += d,
                    d => watcher.Changed -= d))
           .Select(s => HandleChanged(s, newToken));

        var created = PrepareEventStream(
                currentState.DispatchFileSystemEvents,
                Observable.FromEvent<FileSystemEventHandler, FileSystemEventArgs>(
                    a => (_, args) => a(args),
                    d => watcher.Created += d,
                    d => watcher.Created -= d))
           .Select(s => HandleCreated(s, newToken));

        var deleted = PrepareEventStream(
                currentState.DispatchFileSystemEvents,
                Observable.FromEvent<FileSystemEventHandler, FileSystemEventArgs>(
                    a => (_, args) => a(args),
                    d => watcher.Deleted += d,
                    d => watcher.Deleted -= d))
           .Select(s => HandleDelete(s, newToken));

        return currentState with
               {
                   Token = newToken,
                   Subscription = Observable.Merge(rename, changed, created, deleted).ToActor(self).DisposeWith(this),
               };
    }

    private IObservable<StatePair<TData, State>> PrepareEventStream<TData>(IObservable<bool> suspender, IObservable<TData> obs)
        => UpdateAndSyncActor(obs)
           .TakeWhile(suspender)
           .Do(s => s.State.DispatchFileSystemEvents.OnNext(false));

    private IObservable<FileChangeEvent> HandleDelete(StatePair<FileSystemEventArgs, State> arg, int token)
        => from msg in arg
           let fileName = msg.Event.Name
           where !string.IsNullOrWhiteSpace(fileName)
           from mapEntry in msg.State.FileMapping
           where string.Equals(mapEntry.Value, fileName, StringComparison.Ordinal)
           select new FileChangeEvent(token, fileName, mapEntry.Key);

    private IObservable<FileChangeEvent> HandleCreated(StatePair<FileSystemEventArgs, State> arg, int token)
        => from msg in arg
           let fileName = msg.Event.Name
           where !string.IsNullOrWhiteSpace(fileName)
           let originalName = TryExtractOriginalName(fileName)
           where !string.IsNullOrWhiteSpace(originalName)
           from mapentry in msg.State.FileMapping
           where mapentry.Value.Contains(originalName, StringComparison.Ordinal)
           select new FileChangeEvent(token, fileName, mapentry.Key);

    private IObservable<FileChangeEvent> HandleChanged(StatePair<FileSystemEventArgs, State> arg, int token)
        => from msg in arg
           let fileName = msg.Event.Name
           where !string.IsNullOrWhiteSpace(fileName)
           from mapentry in msg.State.FileMapping
           where string.Equals(mapentry.Value, fileName, StringComparison.Ordinal)
           select new FileChangeEvent(token, fileName, mapentry.Key);

    private static IObservable<FileChangeEvent> HandleRename(StatePair<RenamedEventArgs, State> arg, int token)
        => from msg in arg
           let oldName = msg.Event.OldName
           let newName = msg.Event.Name
           where !string.IsNullOrWhiteSpace(oldName) && !string.IsNullOrWhiteSpace(newName)
           from mapentry in msg.State.FileMapping
           where string.Equals(mapentry.Value, oldName, StringComparison.Ordinal)
           select new FileChangeEvent(token, newName, mapentry.Key);

    private string? TryExtractOriginalName(string newName)
    {
        ReadOnlySpan<char> name = Path.GetFileNameWithoutExtension(newName);

        #pragma warning disable EPS06
        if(name.Length < 3 || name.IsWhiteSpace()) return null;
        #pragma warning restore EPS06

        if(char.IsDigit(name[^0]) && char.IsDigit(name[^1]) && name[^2] == 'V')
            return name[..^2].ToString();

        return null;
    }

    private static void CreateOrClearDirectory(string path)
    {
        if(Directory.Exists(path))
        {
            ClearDirecotry(path);

            return;
        }

        Directory.CreateDirectory(path);
    }

    private static void ClearDirecotry(string path)
    {
        foreach (FileSystemInfo info in new DirectoryInfo(path).EnumerateFileSystemInfos())
            switch (info)
            {
                case FileInfo file:
                    file.Delete();

                    break;
                case DirectoryInfo directory:
                    directory.Delete(recursive: true);

                    break;
            }
    }

    public sealed record State(
        // ReSharper disable once NotAccessedPositionalProperty.Global
        string TargetPath, string CurrentPath, ImageManagerMode Mode, DataTransferManager Server, DataTransferManager Self,
        ImmutableDictionary<ProjectFileId, string> FileMapping, FileSystemWatcher Watcher, IDisposable Subscription, Subject<bool> DispatchFileSystemEvents, int Token = 1);

    private sealed record FileChangeEvent(int Token, string FileName, ProjectFileId Id);

    private sealed record SyncFromServer;
}
using System.Collections.Immutable;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Akka.Actor;
using Akka.Cluster.Utility;
using SimpleProjectManager.Shared;
using Tauron;
using Tauron.Application.AkkaNode.Services.FileTransfer;
using Tauron.Features;
using Tauron.ObservableExt;
using Tauron.TAkka;

namespace SimpleProjectManager.Operation.Client.Shared.Core;

public sealed class ImageManagerFeature : ActorFeatureBase<ImageManagerFeature.State>
{
    public sealed record State(
        string TargetPath, string CurrentPath, ImageManagerMode Mode, DataTransferManager Server, DataTransferManager Self,
        ImmutableDictionary<ProjectFileId, string> FileMapping, FileSystemWatcher Watcher, IDisposable Subscription, Subject<bool> DispatchFileSystemEvents, int Token = 1);

    public static IPreparedFeature Create(string targetPath, ImageManagerMode mode)
        => Feature.Create(
            () => new ImageManagerFeature(),
            c => new State(targetPath, string.Empty, mode, DataTransferManager.Empty, DataTransferManager.New(c, "FileTransfer"), 
                ImmutableDictionary<ProjectFileId, string>.Empty, new FileSystemWatcher(targetPath), Disposable.Empty, new Subject<bool>()));

    protected override void ConfigImpl()
    {
        CurrentState.Watcher.EnableRaisingEvents = true;
        CurrentState.Watcher.DisposeWith(this);
        
        ClusterActorDiscovery.Get(Context.System).RegisterActor(new ClusterActorDiscoveryMessage.RegisterActor(Self, nameof(ImageManagerFeature)));
        
        Receive<RegisterServerManager>(
            obs => from msg in obs.Do(m => m.Sender.Tell(new ServerManagerResponse(m.State.Self)))
                   select msg.State with { Server = msg.Event.ServerManager });

        Receive<StartProject>(
            obs => obs.ConditionalSelect()
               .ToResult<State>(
                    b => b
                       .When(s => s.State.Mode == ImageManagerMode.Single, o => o.Select(StartSingle))
                       .When(s => s.State.Mode == ImageManagerMode.Multiple, o => o.Select(StartMultiple))));
    }

    private State StartMultiple(StatePair<StartProject, State> obj)
    {
        var (evt, state) = obj;
        
        try
        {
            var jobName = evt.JobName;
            var targetPath = Path.Combine(state.CurrentPath, jobName);

            var newState = UpdateFileSystemWatcher(state, targetPath);
            CreateOrClearDirectory(targetPath);
            obj.Sender.Tell(new ProjectStartResponse(null));

            return newState with { CurrentPath = targetPath };
        }
        catch (Exception e)
        {
            Log.Error(e, "Error on Start Project with multiple mode: {Name}", evt.JobName);
            obj.Sender.Tell(new ProjectStartResponse(e.Message));
        }

        return obj.State with{CurrentPath = string.Empty};
    }

    private State StartSingle(StatePair<StartProject, State> obj)
    {
        var jobName = obj.Event.JobName;

        try
        {
            CreateOrClearDirectory(obj.State.TargetPath);
            var state = UpdateFileSystemWatcher(obj.State, obj.State.TargetPath);
            
            obj.Sender.Tell(new ProjectStartResponse(null));

            return state with { CurrentPath = obj.State.TargetPath };
        }
        catch (Exception e)
        {
            Log.Error(e, "Error on Start Project with Single Mode {Name}", jobName);
            obj.Sender.Tell(new ProjectStartResponse(e.Message));
        }
        
        return obj.State with{ CurrentPath = string.Empty };
    }

    private State UpdateFileSystemWatcher(State currentState, string newPath)
    {
        var watcher = currentState.Watcher;
        ((IResourceHolder)this).RemoveResource(currentState.Subscription);

        var self = Self;
        var newToken = currentState.Token + 1;
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
                   Subscription = Observable.Merge(rename, changed, created, deleted).ToActor(self).DisposeWith(this)
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
           where mapEntry.Value == fileName
           select new FileChangeEvent(token, fileName, mapEntry.Key);
    
    private IObservable<FileChangeEvent> HandleCreated(StatePair<FileSystemEventArgs, State> arg, int token)
        => from msg in arg
           let fileName = msg.Event.Name
           where !string.IsNullOrWhiteSpace(fileName)
           let originalName = TryExtractOriginalName(fileName)
           where !string.IsNullOrWhiteSpace(originalName)
           from mapentry in msg.State.FileMapping 
           where mapentry.Value.Contains(originalName)
           select new FileChangeEvent(token, fileName, mapentry.Key);
    
    private IObservable<FileChangeEvent> HandleChanged(StatePair<FileSystemEventArgs, State> arg, int token)
        => from msg in arg
           let fileName = msg.Event.Name
           where !string.IsNullOrWhiteSpace(fileName)
           from mapentry in msg.State.FileMapping
           where mapentry.Value == fileName
           select new FileChangeEvent(token, fileName, mapentry.Key);

    private static IObservable<FileChangeEvent> HandleRename(StatePair<RenamedEventArgs, State> arg, int token)
        => from msg in arg
           let oldName = msg.Event.OldName
           let newName = msg.Event.Name
           where !string.IsNullOrWhiteSpace(oldName) && !string.IsNullOrWhiteSpace(newName)
           from mapentry in msg.State.FileMapping
           where mapentry.Value == oldName
           select new FileChangeEvent(token, newName, mapentry.Key);

    private string? TryExtractOriginalName(string newName)
    {
        ReadOnlySpan<char> name = Path.GetFileNameWithoutExtension(newName);

        if (name.Length < 3 || name.IsWhiteSpace()) return null;

        if (char.IsDigit(name[^0]) && char.IsDigit(name[^1]) && name[^2] == 'V')
            return name[..^2].ToString();

        return null;
    }
    
    private static void CreateOrClearDirectory(string path)
    {
        if (Directory.Exists(path))
        {
            ClearDirecotry(path);
            return;
        }

        Directory.CreateDirectory(path);
    }

    private static void ClearDirecotry(string path)
    {
        foreach (var info in new DirectoryInfo(path).EnumerateFileSystemInfos())
        {
            switch (info)
            {
                case FileInfo file:
                    file.Delete();

                    break;
                case DirectoryInfo directory:
                    directory.Delete(true);

                    break;
            }
        }
    }

    private sealed record FileChangeEvent(int token, string FileName, ProjectFileId Id);

    private sealed record SyncFromServer;
}
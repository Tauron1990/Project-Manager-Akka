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
        
            CreateOrClearDirectory(targetPath);
            obj.Sender.Tell(new ProjectStartResponse(null));

            return state with { CurrentPath = targetPath };
        }
        catch (Exception e)
        {
            Log.Error(e, "Error on Start Project with multiple mode: {Name}", evt.JobName);
            obj.Sender.Tell(new ProjectStartResponse(e.Message));
        }

        return obj.State;
    }

    private State StartSingle(StatePair<StartProject, State> obj)
    {
        var jobName = obj.Event.JobName;

        try
        {
            CreateOrClearDirectory(obj.State.TargetPath);
            obj.Sender.Tell(new ProjectStartResponse(null));

            return obj.State with { CurrentPath = obj.State.TargetPath };
        }
        catch (Exception e)
        {
            Log.Error(e, "Error on Start Project with Single Mode {Name}", jobName);
            obj.Sender.Tell(new ProjectStartResponse(e.Message));
        }
        
        return obj.State;
    }

    private State UpdateFileSystemWatcher(State currentState, string newPath, FileSystemWatcher watcher)
    {
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
                d => watcher.Created -= d));

        var deleted = PrepareEventStream(
            currentState.DispatchFileSystemEvents,
            Observable.FromEvent<FileSystemEventHandler, FileSystemEventArgs>(
                a => (_, args) => a(args),
                d => watcher.Deleted += d,
                d => watcher.Deleted -= d));

        return currentState with
               {
                   Token = newToken,
                   Subscription = new CompositeDisposable
                                  {
                                      rename.ToActor(self),
                                      changed.ToActor(self),
                                      created.ToActor(self),
                                      deleted.ToActor(self)
                                  }.DisposeWith(this)
               };
    }

    private IObservable<StatePair<TData, State>> PrepareEventStream<TData>(IObservable<bool> suspender, IObservable<TData> obs)
        => UpdateAndSyncActor(obs)
           .TakeWhile(suspender)
           .Do(s => s.State.DispatchFileSystemEvents.OnNext(false));
    
    private FileChangeEvent HandleChanged(StatePair<FileSystemEventArgs, State> arg, int token)
    {
        var name = arg.Event.Name;

        if (string.IsNullOrWhiteSpace(name)) return null;
    }

    private static IObservable<FileChangeEvent> HandleRename(StatePair<RenamedEventArgs, State> arg, int token)
        => from msg in Observable.Return(arg)
           let oldName = msg.Event.OldName
           let newName = msg.Event.Name
           where !string.IsNullOrWhiteSpace(oldName) && !string.IsNullOrWhiteSpace(newName)
           from mapentry in msg.State.FileMapping
           where mapentry.Value == oldName
           select new FileChangeEvent(token, newName, mapentry.Key);

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
}
using System.Collections.Immutable;
using System.Reactive;
using System.Reactive.Linq;
using Akka.Actor;
using Akka.Cluster.Utility;
using Tauron;
using Tauron.Application.AkkaNode.Services.FileTransfer;
using Tauron.Features;
using Tauron.ObservableExt;

namespace SimpleProjectManager.Operation.Client.Shared.Core;

public sealed class ImageManagerFeature : ActorFeatureBase<ImageManagerFeature.State>
{
    public sealed record State(
        string TargetPath, string CurrentPath, ImageManagerMode Mode, DataTransferManager Server, DataTransferManager Self,
        ImmutableDictionary<string, string> FileMapping, FileSystemWatcher Watcher);

    public static IPreparedFeature Create(string targetPath, ImageManagerMode mode)
        => Feature.Create(
            () => new ImageManagerFeature(),
            c => new State(targetPath, string.Empty, mode, DataTransferManager.Empty, DataTransferManager.New(c, "FileTransfer"), 
                ImmutableDictionary<string, string>.Empty, new FileSystemWatcher(targetPath)));

    protected override void ConfigImpl()
    {
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

    private sealed record FileChangeEvent(string BasePath, string FileName);
}
﻿using System;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Event;
using JetBrains.Annotations;
using ServiceManager.ProjectDeployment.Build;
using ServiceManager.ProjectDeployment.Data;
using Tauron;
using Tauron.Akka;
using Tauron.Application.AkkNode.Services;
using Tauron.Application.AkkNode.Services.FileTransfer;
using Tauron.Application.AkkNode.Services.FileTransfer.TemporarySource;
using Tauron.Application.Master.Commands.Deployment.Build;
using Tauron.Application.Master.Commands.Deployment.Repository;

namespace ServiceManager.ProjectDeployment.Actors
{
    public enum BuildState
    {
        Failing,
        Waiting,
        Repository,
        Extracting,
        Building,
        Compressing
    }

    public sealed class BuildPaths : IDisposable
    {
        private TempFile? _repoFile;
        public string BasePath { get; } = string.Empty;

        public TempFile RepoFile => _repoFile ??= new TempFile(true);

        public string BuildPath { get; } = string.Empty;
        public string RepoPath { get; } = string.Empty;

        public BuildPaths(string basePath)
        {
            BasePath = basePath;
            BuildPath = Path.Combine(basePath, "Build");
            RepoPath = Path.Combine(basePath, "Repository");
            BuildPath.CreateDirectoryIfNotExis();
        }

        public BuildPaths()
        {
            
        }

        public void Dispose()
        {
            RepoFile.ForceDispose();
            BasePath.DeleteDirectory(true);
        }
    }

    public sealed class BuildData
    {
        public Reporter? Reporter { get; private set; }

        public AppData AppData { get; private set; } = AppData.Empty;
        
        public RepositoryApi Api { get; private set; } = RepositoryApi.Empty;

        private IActorRef CurrentListner { get; set; } = ActorRefs.Nobody;

        public string Error { get; private set; } = string.Empty;

        public string OperationId { get; private set; } = string.Empty;

        public BuildPaths Paths { get; private set; } = new BuildPaths();

        public TempFile Target { get; private set; } = null!;

        public string Commit { get; set; } = string.Empty;

        public TaskCompletionSource<(string, TempStream)>? CompletionSource { get; private set; }

        public BuildData Set(BuildRequest request)
        {
            OperationId = Guid.NewGuid().ToString("D");
            Reporter = request.Source;
            AppData = request.AppData;
            Api = request.RepositoryApi;
            Target = request.TargetFile;
            CompletionSource = request.CompletionSource;
            return this;
        }

        public BuildData Set(IncomingDataTransfer request)
        {
            Paths = new BuildPaths(Path.Combine(BuildEnv.ApplicationPath, "Building", OperationId));

            request.Accept(() => Paths.RepoFile.CreateDate());

            return this;
        }

        public BuildData SetError(string error)
        {
            Error = error;
            return this;
        }

        public BuildData SetListner(IActorRef list)
        {
            if (!CurrentListner.IsNobody())
                ExposedReceiveActor.ExposedContext.Stop(CurrentListner);

            Paths.BasePath.DeleteDirectory(true);
            CurrentListner = list;

            return this;
        }

        public BuildData Clear(ILoggingAdapter adapter)
        {
            CompletionSource?.TrySetCanceled();
            ExposedReceiveActor.ExposedContext.Stop(CurrentListner);

            try
            {
                Paths.Dispose();
            }
            catch (Exception e)
            {
                adapter.Error(e, "Error on disposing Build Paths");
            }

            return new BuildData();
        }
    }

    [UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
    public sealed class BuildingActor : FSM<BuildState, BuildData>
    {
        private readonly ILoggingAdapter _log = Context.GetLogger();

        public BuildingActor(DataTransferManager fileHandler)
        {
            fileHandler.Event<TransferCompled>();
            fileHandler.Event<TransferFailed>();

            StartWith(BuildState.Waiting, new BuildData());

            When(BuildState.Waiting, evt =>
            {
                switch (evt.FsmEvent)
                {
                    case TransferFailed _: return Stay();
                    case TransferCompled _: return Stay();
                    case BuildRequest request:
                    {
                        _log.Info("Incomming Build Request {Apps}", request.AppData.Name);
                        var newData = evt.StateData.Set(request);

                        var listner = Reporter.CreateListner(Context, newData.Reporter!, TimeSpan.FromMinutes(5), task => task.PipeTo(Self));

                        newData.Api.SendAction(new TransferRepository(newData.AppData.Repository, listner, fileHandler, newData.OperationId));
                        return GoTo(BuildState.Repository)
                            .Using(newData.SetListner(listner));
                    }
                    default:
                        return null;
                }
            });

            When(BuildState.Repository, evt =>
            {
                switch (evt.FsmEvent)
                {
                    case OperationResult result:
                        if (result.Ok)
                        {
                            if (result.Outcome is Tranferdata data) StateData.Commit = data.Commit;
                            else StateData.Commit = "Unkowen";

                            return Stay();
                        }
                        else
                        {
                            return GoTo(BuildState.Failing)
                               .Using(evt.StateData.SetError(result.Error ?? BuildErrorCodes.GernalBuildError));
                        }
                    case IncomingDataTransfer transfer:
                        _log.Info("Start Repository Transfer {Name}", evt.StateData.AppData.Name);
                        if (transfer.OperationId != evt.StateData.OperationId)
                            return Stay();
                        else
                        {
                            evt.StateData.Reporter?.Send(DeploymentMessages.BuildTransferRepoistory);
                            return Stay().Using(evt.StateData.Set(transfer));
                        }
                    case TransferFailed fail:
                        _log.Warning("Repository Transfer Failed {Name}--{Reason}", evt.StateData.AppData.Name, fail.Reason);
                        if (fail.OperationId != evt.StateData.OperationId)
                            return Stay();
                        return GoTo(BuildState.Failing)
                            .Using(evt.StateData.SetError(fail.Reason.ToString()));
                    case TransferCompled c:
                        _log.Info("Repository Transfer Compled {Name}", evt.StateData.AppData.Name);
                        if (c.OperationId != evt.StateData.OperationId)
                            return Stay();
                        return GoTo(BuildState.Extracting)
                           .ReplyingSelf(Trigger.Inst);
                    default:
                        return null;
                }
            }, TimeSpan.FromMinutes(5));

            When(BuildState.Extracting, evt =>
            {
                switch (evt.FsmEvent)
                {
                    case Trigger _:
                        _log.Info("Extract Repository {Name}", evt.StateData.AppData.Name);
                        var paths = evt.StateData.Paths;
                        evt.StateData.Reporter?.Send(DeploymentMessages.BuildExtractingRepository);
                        Task.Run(() =>
                        {
                            using var archive = new ZipArchive(paths.RepoFile.CreateStream(), ZipArchiveMode.Read);
                            archive.ExtractToDirectory(paths.RepoPath, true);

                        }).PipeTo(Self, success:() => new Status.Success(null));
                        return Stay();
                    case Status.Success _:
                        _log.Info("Repository Extracted {Name}", evt.StateData.AppData.Name);
                        return GoTo(BuildState.Building)
                           .ReplyingSelf(Trigger.Inst);
                    default:
                        return null;
                }
            });

            When(BuildState.Building, evt =>
            {
                switch (evt.FsmEvent)
                {
                    case Trigger _:
                        evt.StateData.Reporter?.Send(DeploymentMessages.BuildRunBuilding);
                        try
                        {
                            _log.Info("Try Find Project {ProjectName} for {Name}", evt.StateData.AppData.ProjectName, evt.StateData.AppData.Name);
                            evt.StateData.Reporter?.Send(DeploymentMessages.BuildTryFindProject);
                            var finder = new ProjectFinder(new DirectoryInfo(evt.StateData.Paths.RepoPath));
                            var file = finder.Search(evt.StateData.AppData.ProjectName);
                            if (file == null)
                            {
                                _log.Warning("Project {ProjectName} Not found for {Name}", evt.StateData.AppData.ProjectName, evt.StateData.AppData.Name);
                                return GoTo(BuildState.Failing).Using(evt.StateData.SetError(BuildErrorCodes.BuildProjectNotFound));
                            }

                            _log.Info("Start Building Task for {Name}", evt.StateData.AppData.Name);

                            Action<string> log;
                            if (evt.StateData.Reporter != null)
                                log = evt.StateData.Reporter.Send;
                            else
                                log = s => { };

                            Task
                               .Run(async () => await DotNetBuilder.BuildApplication(file, evt.StateData.Paths.BuildPath, log))
                               .PipeTo(Self, success: s => string.IsNullOrWhiteSpace(s) ? OperationResult.Success() : OperationResult.Failure(s));

                            return Stay();
                        }
                        catch (Exception e)
                        {
                            evt.StateData.CompletionSource?.TrySetException(e);
                            return GoTo(BuildState.Failing).Using(evt.StateData.SetError(e.Unwrap()?.Message ?? "Unkowen"));
                        }
                    case OperationResult result:
                        return !result.Ok 
                            ? GoTo(BuildState.Failing).Using(StateData.SetError(result.Error ?? string.Empty)) 
                            : GoTo(BuildState.Compressing).ReplyingSelf(Trigger.Inst);
                    default:
                        return null;
                }
            });

            When(BuildState.Compressing, evt =>
            {
                switch (evt.FsmEvent)
                {
                    case Trigger _:
                        Task.Run(() =>
                        {
                            ZipFile.CreateFromDirectory(evt.StateData.Paths.BuildPath, evt.StateData.Target.TargetFile);
                        }).PipeTo(Self, success:() => new Status.Success(null));
                        return Stay();
                    case Status.Success _:
                        evt.StateData.CompletionSource?.SetResult((StateData.Commit, StateData.Target.CreateStream()));
                        return GoTo(BuildState.Waiting)
                            .Using(evt.StateData.Clear(_log))
                            .ReplyingParent(BuildCompled.Inst);
                    default:
                        return null;
                }
            });

            When(BuildState.Failing, evt =>
            {
                evt.StateData.Target.ForceDispose();
                return GoTo(BuildState.Waiting)
                   .Using(evt.StateData.Clear(_log))
                   .ReplyingParent(BuildCompled.Inst);
            });

            WhenUnhandled(evt =>
            {
                switch (evt.FsmEvent)
                {
                    case IncomingDataTransfer _:
                    case TransferFailed _:
                    case TransferCompled _:
                    case OperationResult _:
                        return Stay();
                    case StateTimeout _ when StateName != BuildState.Waiting:
                        _log.Info("Timeout in Building {Name}", evt.StateData.AppData.Name);
                        return GoTo(BuildState.Failing).Using(evt.StateData.SetError(Reporter.TimeoutError));
                    case Status.Failure f when StateName != BuildState.Waiting:
                        _log.Warning("Operation Failed {Name}--{Error}", evt.StateData.AppData.Name, f.Cause.Unwrap()?.Message);
                        evt.StateData.CompletionSource?.TrySetException(f.Cause);
                        return GoTo(BuildState.Failing).Using(evt.StateData.SetError(f.Cause.Unwrap()?.Message ?? "Unkowen"));
                    default:
                        return Stay();
                }
            });

            OnTransition((state, nextState) =>
            {
                switch (nextState)
                {
                    case BuildState.Failing:
                        StateData.Reporter?.Compled(
                            OperationResult.Failure(string.IsNullOrWhiteSpace(StateData.Error)
                            ? BuildErrorCodes.GernalBuildError
                            : StateData.Error));
                        Self.Tell(Trigger.Inst);
                        break;
                }
            });

            OnTermination(evt =>
            {
                _log.Error("Unexpekted Termination {Cause} on {State}", evt.Reason.ToString(), evt.TerminatedState);

                evt.StateData.CompletionSource?.TrySetCanceled();
                if (evt.StateData.Reporter == null || evt.StateData.Reporter.IsCompled) return;
                
                OperationResult result;
                if (evt.Reason is Failure failure)
                    result = OperationResult.Failure(failure.Cause?.ToString() ?? "Unkowen");
                else
                    result = OperationResult.Failure("Unkowen");

                evt.StateData.Reporter.Compled(result);
            });

            Initialize();
        }

        private sealed class Trigger
        {
            public static readonly Trigger Inst = new Trigger();
        }
    }
}
using System;
using System.IO;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Event;
using Ionic.Zip;
using JetBrains.Annotations;
using ServiceManager.ProjectDeployment.Build;
using ServiceManager.ProjectDeployment.Data;
using Tauron;
using Tauron.Akka;
using Tauron.Application.AkkaNode.Services;
using Tauron.Application.AkkaNode.Services.FileTransfer;
using Tauron.Application.AkkaNode.Services.Reporting;
using Tauron.Application.AkkaNode.Services.Reporting.Commands;
using Tauron.Application.Master.Commands.Deployment.Build;
using Tauron.Application.Master.Commands.Deployment.Repository;
using Tauron.Operations;
using Tauron.Temp;

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
        private ITempFile? _repoFile;

        public BuildPaths(ITempDic basePath)
        {
            BasePath = basePath;
            BuildPath = basePath.CreateDic();
            RepoPath = basePath.CreateDic();
        }

        public BuildPaths()
        {
        }

        public ITempDic BasePath { get; } = TempDic.Null;

        public ITempFile RepoFile
        {
            get
            {
                if (_repoFile != null) return _repoFile;

                _repoFile = BasePath.CreateFile();
                _repoFile.NoStreamDispose = true;

                return _repoFile;
            }
        }

        public ITempDic BuildPath { get; } = TempDic.Null;
        public ITempDic RepoPath { get; } = TempDic.Null;

        public void Dispose()
            => BasePath.Dispose();
    }

    public sealed class BuildData
    {
        public Reporter Reporter { get; private set; } = Reporter.Empty;

        public AppData AppData { get; private set; } = AppData.Empty;

        public RepositoryApi Api { get; private set; } = RepositoryApi.Empty;

        private IActorRef CurrentListner { get; set; } = ActorRefs.Nobody;

        public string Error { get; private set; } = string.Empty;

        //public string OperationId { get; private set; } = string.Empty;

        public BuildPaths Paths { get; private set; } = new();

        public ITempFile Target { get; private set; } = null!;

        public string Commit { get; set; } = string.Empty;

        public TaskCompletionSource<(string, ITempFile)>? CompletionSource { get; private set; }

        public BuildData Set(BuildRequest request)
        {
            //OperationId = Guid.NewGuid().ToString("D");
            Paths = new BuildPaths(BuildEnv.TempFiles.CreateDic());
            Reporter = request.Source;
            AppData = request.AppData;
            Api = request.RepositoryApi;
            Target = request.TargetFile;
            CompletionSource = request.CompletionSource;
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
                ObservableActor.ExposedContext.Stop(CurrentListner);

            Paths.BasePath.Clear();
            CurrentListner = list;

            return this;
        }

        public BuildData Clear(ILoggingAdapter adapter)
        {
            CompletionSource?.TrySetCanceled();
            ObservableActor.ExposedContext.Stop(CurrentListner);

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
            StartWith(BuildState.Waiting, new BuildData());

            When(BuildState.Waiting,
                evt =>
                {
                    switch (evt.FsmEvent)
                    {
                        case TransferFailed: return Stay();
                        case TransferMessages.TransferCompled: return Stay();
                        case BuildRequest request:
                        {
                            _log.Info("Incomming Build Request {Apps}", request.AppData.Id);
                            var newData = evt.StateData.Set(request);
                            newData.Api.Send(new TransferRepository(newData.AppData.Repository), TimeSpan.FromMinutes(5), fileHandler, newData.Reporter.Send, () => newData.Paths.RepoFile.Stream)
                                   .PipeTo(Self);

                            return GoTo(BuildState.Repository)
                               .Using(newData.SetListner(ActorRefs.Nobody));
                        }
                        default:
                            return null;
                    }
                });

            When(BuildState.Repository,
                evt =>
                {
                    switch (evt.FsmEvent)
                    {
                        case TransferFailed fail:
                            _log.Warning("Repository Transfer Failed {Name}--{Reason}", evt.StateData.AppData.Id,
                                fail.Reason);
                            //if (fail.OperationId != evt.StateData.OperationId)
                            //    return Stay();
                            return GoTo(BuildState.Failing)
                               .Using(evt.StateData.SetError(fail.Reason.ToString()));
                        case TransferMessages.TransferCompled c:
                            _log.Info("Repository Transfer Compled {Name}", evt.StateData.AppData.Id);
                            //if (c.OperationId != evt.StateData.OperationId)
                            //    return Stay();
                            evt.StateData.Commit = c.Data ?? "Unkowen";
                            return GoTo(BuildState.Extracting)
                               .ReplyingSelf(Trigger.Inst);
                        default:
                            return null;
                    }
                }, TimeSpan.FromMinutes(5));

            When(BuildState.Extracting,
                evt =>
                {
                    switch (evt.FsmEvent)
                    {
                        case Trigger:
                            _log.Info("Extract Repository {Name}", evt.StateData.AppData.Id);
                            var paths = evt.StateData.Paths;
                            evt.StateData.Reporter.Send(DeploymentMessages.BuildExtractingRepository);
                            Task.Run(() =>
                                     {
                                         var stream = paths.RepoFile.Stream;
                                         stream.Seek(0, SeekOrigin.Begin);
                                         using var archive = ZipFile.Read(stream);
                                         archive.ExtractAll(paths.RepoPath.FullPath, ExtractExistingFileAction.OverwriteSilently);
                                     }).PipeTo(Self, 
                                success: () => new Status.Success(null),
                                failure: e => new Status.Failure(e));
                            return Stay();
                        case Status.Success:
                            _log.Info("Repository Extracted {Name}", evt.StateData.AppData.Id);
                            return GoTo(BuildState.Building)
                               .ReplyingSelf(Trigger.Inst);
                        case Status.Failure f:
                            _log.Warning(f.Cause, "Repository Extraction Failed {Name}", evt.StateData.AppData.Id);
                            return GoTo(BuildState.Failing)
                                  .Using(evt.StateData.SetError(f.Cause.Message))
                                  .ReplyingSelf(Trigger.Inst);
                        default:
                            return null;
                    }
                });

            When(BuildState.Building,
                evt =>
                {
                    switch (evt.FsmEvent)
                    {
                        case Trigger:
                            evt.StateData.Reporter.Send(DeploymentMessages.BuildRunBuilding);
                            try
                            {
                                _log.Info("Try Find Project {ProjectName} for {Name}", evt.StateData.AppData.ProjectName,
                                    evt.StateData.AppData.Id);
                                evt.StateData.Reporter.Send(DeploymentMessages.BuildTryFindProject);
                                var finder = new ProjectFinder(new DirectoryInfo(evt.StateData.Paths.RepoPath.FullPath));
                                var file = finder.Search(evt.StateData.AppData.ProjectName);
                                if (file == null)
                                {
                                    _log.Warning("Project {ProjectName} Not found for {Name}",
                                        evt.StateData.AppData.ProjectName, evt.StateData.AppData.Id);
                                    return GoTo(BuildState.Failing)
                                       .Using(evt.StateData.SetError(BuildErrorCodes.BuildProjectNotFound));
                                }

                                _log.Info("Start Building Task for {Name}", evt.StateData.AppData.Id);

                                Action<string> log = evt.StateData.Reporter.Send;

                                Task.Run(async ()
                                             => await DotNetBuilder.BuildApplication(file, evt.StateData.Paths.BuildPath.FullPath, log))
                                    .PipeTo(Self,
                                         success: s => string.IsNullOrWhiteSpace(s)
                                                      ? OperationResult.Success()
                                                      : OperationResult.Failure(s));

                                return Stay();
                            }
                            catch (Exception e)
                            {
                                evt.StateData.CompletionSource?.TrySetException(e);
                                return GoTo(BuildState.Failing)
                                   .Using(evt.StateData.SetError(e.Unwrap()?.Message ?? "Unkowen"));
                            }
                        case IOperationResult result:
                            return !result.Ok
                                ? GoTo(BuildState.Failing).Using(StateData.SetError(result.Error ?? string.Empty))
                                : GoTo(BuildState.Compressing).ReplyingSelf(Trigger.Inst);
                        default:
                            return null;
                    }
                });

            When(BuildState.Compressing,
                evt =>
                {
                    switch (evt.FsmEvent)
                    {
                        case Trigger:
                            Task.Run(() =>
                                     {
                                         using var zip = new ZipFile();
                                         zip.AddDirectory(evt.StateData.Paths.BuildPath.FullPath);
                                         zip.Save(evt.StateData.Target.NoDisposeStream);
                                     }).PipeTo(Self, 
                                success: () => new Status.Success(null),
                                failure:e => new Status.Failure(e));
                            return Stay();
                        case Status.Success:
                            evt.StateData.CompletionSource?.SetResult((StateData.Commit, StateData.Target));
                            return GoTo(BuildState.Waiting)
                                  .Using(evt.StateData.Clear(_log))
                                  .ReplyingParent(BuildCompled.Inst);
                        case Status.Failure f:
                            _log.Warning(f.Cause, "Repository Extraction Failed {Name}", evt.StateData.AppData.Id);
                            return GoTo(BuildState.Failing)
                                  .Using(evt.StateData.SetError(f.Cause.Message))
                                  .ReplyingSelf(Trigger.Inst);
                        default:
                            return null;
                    }
                });

            When(BuildState.Failing,
                evt =>
                {
                    evt.StateData.Target.Dispose();
                    evt.StateData.CompletionSource?.SetException(new InvalidOperationException(evt.StateData.Error));
                    return GoTo(BuildState.Waiting)
                          .Using(evt.StateData.Clear(_log))
                          .ReplyingParent(BuildCompled.Inst);
                });

            WhenUnhandled(evt =>
                          {
                              switch (evt.FsmEvent)
                              {
                                  case IncomingDataTransfer:
                                  case TransferFailed:
                                  case TransferMessages.TransferCompled:
                                  case IOperationResult:
                                      return Stay();
                                  case StateTimeout when StateName != BuildState.Waiting:
                                      _log.Info("Timeout in Building {Name}", evt.StateData.AppData.Id);
                                      return GoTo(BuildState.Failing).Using(evt.StateData.SetError(Reporter.TimeoutError));
                                  case Status.Failure f when StateName != BuildState.Waiting:
                                      _log.Warning("Operation Failed {Name}--{Error}", evt.StateData.AppData.Id,
                                          f.Cause.Unwrap()?.Message);
                                      evt.StateData.CompletionSource?.TrySetException(f.Cause);
                                      return GoTo(BuildState.Failing)
                                         .Using(evt.StateData.SetError(f.Cause.Unwrap()?.Message ?? "Unkowen"));
                                  default:
                                      return Stay();
                              }
                          });

            OnTransition((_, nextState) =>
                         {
                             if (nextState == BuildState.Failing)
                             {
                                 if (!StateData.Reporter.IsCompled)
                                 {
                                     StateData.Reporter.Compled(
                                         OperationResult.Failure(string.IsNullOrWhiteSpace(StateData.Error)
                                             ? BuildErrorCodes.GernalBuildError
                                             : StateData.Error));
                                 }

                                 Self.Tell(Trigger.Inst);
                             }
                         });
            
            OnTermination(evt =>
                          {
                              if(evt.Reason != Shutdown.Instance) return;
                                _log.Error("Unexpected Termination {Cause} on {State}", evt.Reason.ToString(), evt.TerminatedState);

                              evt.StateData.Paths.Dispose();

                              evt.StateData.CompletionSource?.TrySetCanceled();
                              if (evt.StateData.Reporter.IsCompled) return;

                              IOperationResult result;
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
            public static readonly Trigger Inst = new();
        }
    }
}
using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using Akka.Actor;
using JetBrains.Annotations;
using ServiceManager.ProjectDeployment.Build;
using ServiceManager.ProjectDeployment.Data;
using SharpRepository.Repository;
using Tauron;
using Tauron.Application.AkkaNode.Services;
using Tauron.Application.AkkaNode.Services.CleanUp;
using Tauron.Application.AkkaNode.Services.Commands;
using Tauron.Application.AkkaNode.Services.FileTransfer;
using Tauron.Application.Files.VirtualFiles;
using Tauron.Application.Master.Commands.Deployment.Build;
using Tauron.Application.Master.Commands.Deployment.Build.Commands;
using Tauron.Application.Master.Commands.Deployment.Build.Data;
using Tauron.Application.Master.Commands.Deployment.Repository;
using Tauron.Application.Workshop;
using Tauron.Features;
using Tauron.ObservableExt;
using Tauron.Operations;
using Tauron.Temp;

namespace ServiceManager.ProjectDeployment.Actors
{
    public sealed class AppCommandProcessor : ReportingActor<AppCommandProcessor.AppCommandProcessorState>
    {
        public static IPreparedFeature New(IRepository<AppData, string> apps, IDirectory files, RepositoryApi repository,
            DataTransferManager dataTransfer, IRepository<ToDeleteRevision, string> toDelete, IWorkDistributor<BuildRequest> workDistributor,
            IActorRef changeTracker)
            => Feature.Create(() => new AppCommandProcessor(),
                _ => new AppCommandProcessorState(apps, files, repository, dataTransfer, toDelete, workDistributor,
                    changeTracker));

        protected override void ConfigImpl()
        {
            CommandPhase1<CreateAppCommand>("CreateApp-Phase 1",
                obs => obs.Do(m => m.Reporter.Send(DeploymentMessages.RegisterRepository))
                          .Select(m => Msg.New(new RegisterRepository(m.Event.TargetRepo, true), m)),
                (command, reporter, op) => new ContinueCreateApp(op, command, reporter));
            
            CommandPhase2<ContinueCreateApp, CreateAppCommand, AppInfo>("CreateApp-Phase 2",
                obs => obs.ConditionalSelect()
                          .ToResult<AppInfo?>(b =>
                                              {
                                                  b.When(m => !m.Result.Ok, o => o.ApplyWhen(m => !m.Reporter.IsCompled, 
                                                                                       data => data.Reporter.Compled(OperationResult.Failure(data.Result.Error ?? 
                                                                                                                                             BuildErrorCodes.CommandErrorRegisterRepository)))
                                                                                  .Select(_ => default(AppInfo)));

                                                  b.When(m => m.AppData != null, o => o.Do(m => m.Reporter.Compled(OperationResult.Failure(BuildErrorCodes.CommandDuplicateApp)))
                                                                                       .Select(_ => default(AppInfo)));

                                                  b.When(m => m.AppData == null,
                                                      o => o.Select(m =>
                                                                    {
                                                                        var data = new AppData(
                                                                            ImmutableList<AppFileInfo>.Empty, m.Command.AppName, -1, DateTime.UtcNow,
                                                                            DateTime.MinValue, m.Command.TargetRepo, m.Command.ProjectName);
                                                                        m.State.Apps.Add(data);
                                                                        return (Data: data.ToInfo(), Tracker: m.State.ChangeTracker);
                                                                    })
                                                            .Do(i => i.Tracker.Tell(i.Data))
                                                            .Select(i => i.Data));
                                              }));
            
            DirectCommandPhase1<PushVersionCommand>("PushVersion-Phase 1",
                obs => obs.Select(m => (App:m.State.Apps.Get(m.Event.AppName), Data:m))
                          .ConditionalSelect()
                          .ToResult<Unit>(
                               b =>
                               {
                                   b.When(m => m.App == null, o => o.ToUnit(d => d.Data.Reporter.Compled(OperationResult.Failure(BuildErrorCodes.CommandAppNotFound))));

                                   b.When(m => m.App != null,
                                       o => o.ToUnit(d => BuildRequest.SendWork(d.Data.State.WorkDistributor, d.Data.Reporter, d.App, d.Data.State.Repository, BuildEnv.TempFiles.CreateFile())
                                                                      .PipeTo(Self,
                                                                           success: c => new ContinuePushNewVersion(OperationResult.Success(c), d.Data.Event, d.Data.Reporter),
                                                                           failure: e => new ContinuePushNewVersion(OperationResult.Failure(e), d.Data.Event, d.Data.Reporter))));
                               }));

            CommandPhase2<ContinuePushNewVersion, PushVersionCommand, AppBinary>("PushVersion-Phase 2",
                obs => obs.ConditionalSelect()
                          .ToResult<AppBinary?>(
                               b =>
                               {
                                   b.When(m => m.AppData == null, o => o.Do(m => m.Reporter.Compled(OperationResult.Failure(BuildErrorCodes.CommandAppNotFound)))
                                                                        .Select(_ => default(AppBinary)));

                                   b.When(m => m.AppData != null, o => o.SelectMany(UpdateAppData));

                                   static IObservable<AppBinary?> UpdateAppData(ContinueData<PushVersionCommand> m)
                                   {
                                       var (commit, fileName) = ((string, ITempFile))m.Result.Outcome!;
                                       return Observable.Using(() => fileName,
                                           file =>
                                               (
                                                   from info in Observable.Return((Commit: commit, File: file, Data: m))
                                                   let oldApp = info.Data.AppData ?? AppData.Empty
                                                   let newVersion = oldApp.Last + 1
                                                   let newId = $"{oldApp.Id}-{newVersion}.zip"
                                                   let newBinary = new AppFileInfo(newId, oldApp.Last + 1, DateTime.UtcNow, false, info.Commit)
                                                   let newData = oldApp with
                                                                 {
                                                                     Last = newVersion,
                                                                     LastUpdate = DateTime.UtcNow,
                                                                     Versions = info.Data.AppData!.Versions.Add(newBinary)
                                                                 }
                                                   select (info.Data.State, NewData: newData, NewBinary: newBinary, info.File,
                                                           ToDelete: newData.Versions.OrderByDescending(i => i.CreationTime).Skip(5)
                                                                            .Where(i => !i.Deleted).ToArray())
                                               )
                                              .Select(i =>
                                                      {
                                                          using var stream = i.State.Files.GetFile(i.NewBinary.Id).Open(FileAccess.Write);
                                                          using var fileStream = i.File.Stream;

                                                          fileStream.CopyTo(stream);

                                                          var newData = i.ToDelete
                                                                         .Aggregate(i.NewData,
                                                                              (current, appFileInfo) => current! with
                                                                                                        {
                                                                                                            Versions = current.Versions
                                                                                                                              .Replace(appFileInfo, appFileInfo with
                                                                                                                                                    {
                                                                                                                                                        Deleted = true
                                                                                                                                                    })
                                                                                                        });

                                                          i.State.Apps.Update(newData!);
                                                          i.State.ToDelete.Add(i.ToDelete.Select(e => new ToDeleteRevision(e.Id)));
                                                          return (Data:newData, i.NewBinary, i.State);
                                                      })
                                              .Do(i => i.State.ChangeTracker.Tell(i.Data!.ToInfo()))
                                              .Select(i => new AppBinary(
                                                          i.NewBinary.Version, i.Data!.Id, i.NewBinary.CreationTime, false,
                                                          i.NewBinary.Commit, i.Data.Repository)));
                                   }
                               }));

            DirectCommandPhase1<DeleteAppCommand>("DeleteApp",
                obs => obs.Select(m => m.New((Command:m.Event, App:m.State.Apps.Get(m.Event.AppName))))
                          .ConditionalSelect()
                          .ToResult<Unit>(
                               b =>
                               {
                                   b.When(m => m.Event.App == null, o => o.ToUnit(i => i.Reporter.Compled(OperationResult.Failure(BuildErrorCodes.CommandAppNotFound))));

                                   b.When(m => m.Event.App != null,
                                       o => o.ToUnit(i =>
                                                     {
                                                         i.State.Apps.Delete(i.Event.App);
                                                         i.State.ToDelete.Add(i.Event.App.Versions.Select(d => new ToDeleteRevision(d.Id)));

                                                         i.Reporter.Compled(OperationResult.Success(i.Event.App.ToInfo().IsDeleted()));
                                                     }));
                               }));

            DirectCommandPhase1<ForceBuildCommand>("ForceBuild-Phase 1",
                obs => obs.Select(i => i.New((Command:i.Event, App:new AppData(ImmutableList<AppFileInfo>.Empty, i.Event.AppName, -1, DateTime.UtcNow, DateTime.MinValue, 
                                                  i.Event.Repository, i.Event.Project))))
                          .ToUnit(i => BuildRequest.SendWork(i.State.WorkDistributor, i.Reporter, i.Event.App, i.State.Repository, BuildEnv.TempFiles.CreateFile())
                                                   .PipeTo(Self,
                                                        success:d => new ContinueForceBuild(OperationResult.Success(d.File), i.Event.Command, i.Reporter, i.Event.App),
                                                        failure:e => new ContinueForceBuild(OperationResult.Failure(e), i.Event.Command, i.Reporter, i.Event.App))));

            CommandPhase2<ContinueForceBuild, ForceBuildCommand, FileTransactionId>("ForceBuild-Phase 2",
                obs => obs.ConditionalSelect()
                          .ToResult<FileTransactionId?>(
                               b =>
                               {
                                   b.When(m => m.Command.Manager == null || m.Result.Outcome is not ITempFile, o => o.Select(_ => default(FileTransactionId)));
                                   b.When(m => !m.Result.Ok, o => o.ApplyWhen(d => !d.Reporter.IsCompled, data => data.Reporter.Compled(data.Result))
                                                                   .Select(_ => default(FileTransactionId)));

                                   b.When(m => m.Result.Ok && m.Result.Outcome is ITempFile && m.Command.Manager != null,
                                       o => o.SelectMany(
                                           continueData => Observable.Using(
                                               () => (ITempFile) continueData.Result.Outcome!,
                                               file => Observable.Return((File: file, Target: continueData.Command.GetTransferManager(), Manager: continueData.State.DataTransfer))
                                                                 .Select(f => f.Manager.Request(DataTransferRequest.FromStream(f.File.Stream, f.Target))))));
                               }),
                r => r.Event.TempData);
        }

        private void CommandPhase1<TCommand>(
            string name, 
            Func<IObservable<ReporterEvent<TCommand, AppCommandProcessorState>>, IObservable<NewMessage<TCommand>>> executor,
            Func<TCommand, Reporter, IOperationResult, object> result)

            where TCommand : ReporterCommandBase<DeploymentApi, TCommand>, IDeploymentCommand
        {
            TryReceive<TCommand>(name,
                obs => obs.SelectMany(
                    reporterEvent => executor(Observable.Return(reporterEvent))
                       .ConditionalSelect()
                        .ToResult<Unit>(
                             b =>
                             {
                                 b.When(m => m.Message == null,
                                     o => o.ToUnit(_ =>
                                                   {
                                                       if(reporterEvent.Reporter.IsCompled)
                                                           reporterEvent.Reporter.Compled(OperationResult.Failure(BuildErrorCodes.GerneralCommandError));
                                                       Log.Info("Command Phase 1 {Command} Failed", typeof(TCommand).Name);
                                                   }));

                                 b.When(m => m.Message != null,
                                     o => o.ToUnit(r =>
                                                   {
                                                       var (msg, command, state) = r;
                                                       Log.Info("Command Phase 1 {Command} -- {Action}", typeof(TCommand).Name, msg!.GetType().Name);

                                                       msg.SetListner(Reporter.CreateListner(Context, reporterEvent.Reporter, TimeSpan.FromSeconds(20), 
                                                           task => task.PipeTo(Self, Sender,
                                                               t => result(command, reporterEvent.Reporter, t),
                                                               e => result(command, reporterEvent.Reporter, OperationResult.Failure(e)))));

                                                       msg.ValidateApi(state.Repository.GetType());
                                                       ((ISender)state.Repository).SendCommand(msg);
                                                   }));
                             })));
        }

        private void DirectCommandPhase1<TCommand>(string name, Func<IObservable<ReporterEvent<TCommand, AppCommandProcessorState>>, IObservable<Unit>> executor)
            where TCommand : ReporterCommandBase<DeploymentApi, TCommand>, IDeploymentCommand
            => TryReceive(name, executor);


        private void CommandPhase2<TContinue, TCommand, TResult>(
            string name, Func<IObservable<ContinueData<TCommand>>, IObservable<TResult?>> executor, 
            Func<ReporterEvent<TContinue, AppCommandProcessorState>, AppData?>? queryApp = null)
            where TContinue : ContinueCommand<TCommand>
            where TCommand : ReporterCommandBase<DeploymentApi, TCommand>, IDeploymentCommand
            where TResult : class
        {
            queryApp ??= evt => QueryApp(evt.State.Apps, evt.Event.Command.AppName);

            TryContinue<TContinue>(name,
                obs => obs.SelectMany(
                    evt => executor(Observable.Return(
                               new ContinueData<TCommand>(evt.Event.Command, evt.Event.Result, evt.Reporter, queryApp(evt), evt.State)))
                          .Where(_ => !evt.Reporter.IsCompled)
                          .ToUnit(result => evt.Reporter.Compled(result == null ? OperationResult.Failure(BuildErrorCodes.GerneralCommandError) : OperationResult.Success(result)))));
        }


        private static AppData? QueryApp(ICrudRepository<AppData, string> collection, string name)
            => collection.Get(name)!;

        public sealed record AppCommandProcessorState(
            IRepository<AppData, string> Apps, IDirectory Files,
            RepositoryApi Repository, DataTransferManager DataTransfer,
            IRepository<ToDeleteRevision, string> ToDelete, IWorkDistributor<BuildRequest> WorkDistributor,
            IActorRef ChangeTracker);

        private static class Msg
        {
            public static NewMessage<TCommand> New<TCommand>(
                IReporterMessage? message,
                ReporterEvent<TCommand, AppCommandProcessorState> pair)
                => new(message, pair.Event, pair.State);
        }

        private sealed record NewMessage<TCommand>(
            IReporterMessage? Message, TCommand Command,
            AppCommandProcessorState State);

        private sealed record ContinueData<TCommand>(TCommand Command, IOperationResult Result, Reporter Reporter, AppData? AppData, AppCommandProcessorState State)
            where TCommand : ReporterCommandBase<DeploymentApi, TCommand>, IDeploymentCommand;

        private abstract record ContinueCommand<TCommand>(IOperationResult Result, TCommand Command, Reporter Reporter) : IDelegatingMessage
            where TCommand : IReporterMessage
        {
            public string Info => Command.Info;
        }

        private sealed record ContinueCreateApp : ContinueCommand<CreateAppCommand>
        {
            public ContinueCreateApp(IOperationResult result, CreateAppCommand command, Reporter reporter)
                : base(result, command, reporter)
            {
            }
        }

        private sealed record ContinuePushNewVersion : ContinueCommand<PushVersionCommand>
        {
            public ContinuePushNewVersion(
                [NotNull] IOperationResult result, PushVersionCommand command,
                [NotNull] Reporter reporter)
                : base(result, command, reporter)
            {
            }
        }

        private sealed record ContinueForceBuild : ContinueCommand<ForceBuildCommand>
        {
            public AppData TempData { get; }

            public ContinueForceBuild(
                IOperationResult result, ForceBuildCommand command,
                Reporter reporter, AppData tempData) : base(result, command, reporter)
            {
                TempData = tempData;
            }
        }
    }
}
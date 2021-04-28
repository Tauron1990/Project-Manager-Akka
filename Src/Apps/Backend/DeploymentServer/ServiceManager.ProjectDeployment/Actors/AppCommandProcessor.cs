using System;
using System.Collections.Immutable;
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
        public static IPreparedFeature New(IRepository<AppData, string> apps, IVirtualFileSystem files, RepositoryApi repository,
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
                                               select (info.Data.State, NewData: newData, NewBinary: newBinary,
                                                       ToDelete: newData.Versions.OrderByDescending(i => i.CreationTime).Skip(5)
                                                                       .Where(i => !i.Deleted))
                                           )
                                          .Select(i =>
                                          {

                                              return i;
                                          })
                                          .Select(i => new AppBinary(
                                                      i.NewBinary.Version, i.NewData!.Id, i.NewBinary.CreationTime, false,
                                                      i.NewBinary.Commit, i.NewData.Repository)));
                                   }
                               }));


            //        var newId = files.UploadFromStream(data.Name + ".zip", targetStream.Stream);

            //        var newBinary = new AppFileInfo(newId, data.Last + 1, DateTime.UtcNow, false, commit);
            //        var newBinarys = data.Versions.Add(newBinary);

            //        var definition = Builders<AppData>.Update;
            //        var updates = new List<UpdateDefinition<AppData>>
            //                      {
            //                          definition.Set(ad => ad.Last, newBinary.Version),
            //                          definition.Set(ad => ad.Versions, newBinarys)
            //                      };

            //        var deleteUpdates = new List<ToDeleteRevision>();

            //        if (data.Versions.Count(s => !s.Deleted) > 5)
            //            foreach (var info in newBinarys.OrderByDescending(i => i.CreationTime).Skip(5))
            //            {
            //                if (info.Deleted) continue;
            //                info.Deleted = true;
            //                deleteUpdates.Add(new ToDeleteRevision(info.File.ToString()));
            //            }

            //        transaction.StartTransaction();

            //        if (deleteUpdates.Count != 0)
            //            toDelete.InsertMany(transaction, deleteUpdates);
            //        if (!apps.UpdateOne(transaction, dataFilter, definition.Combine(updates)).IsAcknowledged)
            //        {
            //            transaction.AbortTransaction();
            //            reporter.Compled(OperationResult.Failure(BuildErrorCodes.DatabaseError));
            //            return null;
            //        }

            //        transaction.CommitTransaction();

            //        changeTracker.Tell(_apps.AsQueryable().FirstOrDefault(ad => ad.Name == command.AppName));
            //        return new AppBinary(command.AppName, newBinary.Version, newBinary.CreationTime, false,
            //            newBinary.Commit, data.Repository);
            //    });
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


        private void CommandPhase2<TContinue, TCommand, TResult>(string name, Func<IObservable<ContinueData<TCommand>>, IObservable<TResult?>> executor)
            where TContinue : ContinueCommand<TCommand>
            where TCommand : ReporterCommandBase<DeploymentApi, TCommand>, IDeploymentCommand
            where TResult : class
        {
            TryContinue<TContinue>(name,
                obs => obs.SelectMany(
                    evt => executor(Observable.Return(
                               new ContinueData<TCommand>(evt.Event.Command, evt.Event.Result, evt.Reporter, QueryApp(evt.State.Apps, evt.Event.Command.AppName), evt.State)))
                          .Where(_ => !evt.Reporter.IsCompled)
                          .ToUnit(result => evt.Reporter.Compled(result == null ? OperationResult.Failure(BuildErrorCodes.GerneralCommandError) : OperationResult.Success(result)))));
        }


        private static AppData? QueryApp(IRepository<AppData, string> collection, string name)
            => collection.Get(name)!;

        public sealed record AppCommandProcessorState(
            IRepository<AppData, string> Apps, IVirtualFileSystem Files,
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
            public ContinueForceBuild(
                [NotNull] IOperationResult result, ForceBuildCommand command,
                [NotNull] Reporter reporter) : base(result, command, reporter)
            {
            }
        }
    }

    public sealed class AppCommandProcessorOld
    {
        public AppCommandProcessorOld()
        {
            //CommandPhase1<DeleteAppCommand>("DeleteApp", (command, reporter) =>
            //                                             {
            //                                                 var data = apps.AsQueryable()
            //                                                                .FirstOrDefault(
            //                                                                     d => d.Name == command.AppName);
            //                                                 if (data == null)
            //                                                 {
            //                                                     reporter.Compled(
            //                                                         OperationResult.Failure(BuildErrorCodes
            //                                                            .CommandAppNotFound));
            //                                                     return;
            //                                                 }

            //                                                 var transaction = apps.Database.Client.StartSession();
            //                                                 transaction.StartTransaction();

            //                                                 var arr = data.Versions.Where(f => f.Deleted).ToArray();
            //                                                 if (arr.Length > 0)
            //                                                     toDelete.InsertMany(transaction,
            //                                                         arr.Select(
            //                                                             f => new ToDeleteRevision(f.File.ToString())));
            //                                                 apps.DeleteOne(transaction,
            //                                                     Builders<AppData>.Filter.Eq(a => a.Name, data.Name));

            //                                                 transaction.CommitTransaction();
            //                                                 reporter.Compled(
            //                                                     OperationResult.Success(data.ToInfo().IsDeleted()));
            //                                             });

            //CommandPhase1<ForceBuildCommand>("ForceBuild", (command, reporter) =>
            //                                               {
            //                                                   var tempData = new AppData(command.AppName, -1,
            //                                                       DateTime.Now, DateTime.MinValue, command.Repository,
            //                                                       command.Repository,
            //                                                       ImmutableList<AppFileInfo>.Empty);
            //                                                   BuildRequest
            //                                                      .SendWork(workDistributor, reporter, tempData,
            //                                                           repository, BuildEnv.TempFiles.CreateFile())
            //                                                      .PipeTo(Self,
            //                                                           success: d => new ContinueForceBuild(
            //                                                               OperationResult.Success(d.Item2),
            //                                                               command, reporter),
            //                                                           failure: e => new ContinueForceBuild(
            //                                                               OperationResult.Failure(
            //                                                                   e.Unwrap()?.Message ?? "Cancel"),
            //                                                               command, reporter));
            //                                               });

            //CommandPhase2<ContinueForceBuild, ForceBuildCommand, FileTransactionId>("ForceBuild2",
            //    (command, result, reporter, _) =>
            //    {
            //        if (!result.Ok || command.Manager == null)
            //            return null;

            //        if (!(result.Outcome is TempStream target)) return null;

            //        var request = DataTransferRequest.FromStream(target, command.Manager);
            //        dataTransfer.Request(request);

            //        return new FileTransactionId(request.OperationId);
            //    });
        }
    }
}
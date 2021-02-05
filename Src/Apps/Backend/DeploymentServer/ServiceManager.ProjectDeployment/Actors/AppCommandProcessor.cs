using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Akka.Actor;
using JetBrains.Annotations;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using ServiceManager.ProjectDeployment.Build;
using ServiceManager.ProjectDeployment.Data;
using Tauron;
using Tauron.Application.AkkaNode.Services;
using Tauron.Application.AkkaNode.Services.CleanUp;
using Tauron.Application.AkkaNode.Services.Commands;
using Tauron.Application.AkkaNode.Services.FileTransfer;
using Tauron.Application.Master.Commands.Deployment.Build;
using Tauron.Application.Master.Commands.Deployment.Build.Commands;
using Tauron.Application.Master.Commands.Deployment.Build.Data;
using Tauron.Application.Master.Commands.Deployment.Repository;
using Tauron.Features;
using Tauron.ObservableExt;
using Tauron.Operations;
using Tauron.Temp;

namespace ServiceManager.ProjectDeployment.Actors
{
    public sealed class AppCommandProcessor : ReportingActor<AppCommandProcessor.AppCommandProcessorState>
    {
        public static IPreparedFeature New(
            IMongoCollection<AppData> apps, GridFSBucket files, RepositoryApi repository,
            DataTransferManager dataTransfer,
            IMongoCollection<ToDeleteRevision> toDelete, IWorkDistributor<BuildRequest> workDistributor,
            IActorRef changeTracker)
            => Feature.Create(() => new AppCommandProcessor(),
                _ => new AppCommandProcessorState(apps, files, repository, dataTransfer, toDelete, workDistributor,
                    changeTracker));

        protected override void Config()
        {
            CommandPhase1<CreateAppCommand>("CreateApp",
                (reporter, observable) => observable.Do(_ => reporter.Send(DeploymentMessages.RegisterRepository))
                                                    .Select(m => Msg.New(
                                                                new RegisterRepository(m.Event.TargetRepo, true), m)),
                (command, reporter, op) => new ContinueCreateApp(op, command, reporter));

            CommandPhase2<ContinueCreateApp, CreateAppCommand, AppInfo>("CreateApp-Phase 2",
                observable => observable
                   .SelectMany(async m =>
                               {
                                   var ((appName, targetRepo, projectName), result, reporter, data, state) = m;

                                   if (!result.Ok)
                                   {
                                       if (reporter.IsCompled) return null;
                                       reporter.Compled(
                                           OperationResult.Failure(result.Error ??
                                                                   BuildErrorCodes.CommandErrorRegisterRepository));
                                       return null;
                                   }

                                   if (data != null)
                                   {
                                       reporter.Compled(OperationResult.Failure(BuildErrorCodes.CommandDuplicateApp));
                                       return null;
                                   }

                                   var newData = new AppData(appName, -1, DateTime.UtcNow, DateTime.MinValue,
                                       targetRepo,
                                       projectName,
                                       ImmutableList<AppFileInfo>.Empty);

                                   await state.Apps.InsertOneAsync(newData);
                                   var info = newData.ToInfo();

                                   state.ChangeTracker.Tell(info);
                                   return info;
                               }));

            DirectCommandPhase1<PushVersionCommand>("PushVersion",
                (reporter, input) => input.Select(m => new
                                                       {
                                                           App = m.State.Apps.AsQueryable()
                                                                  .FirstOrDefault(ad => ad.Name == m.Event.AppName),
                                                           Data = m
                                                       })
                                          .ConditionalSelect()
                                          .ToResult<Unit>(s =>
                                                          {
                                                              s.When(p => p.App == null,
                                                                  resultObs => resultObs
                                                                     .ToUnit(() =>
                                                                                 reporter.Compled(
                                                                                     OperationResult.Failure(
                                                                                         BuildErrorCodes
                                                                                            .CommandAppNotFound))));

                                                              s.When(p => p.App != null,
                                                                      resultObs => resultObs
                                                                         .ToUnit(m => BuildRequest
                                                                             .SendWork(m.Data.State.WorkDistributor,
                                                                                  reporter, m.App!,
                                                                                  m.Data.State.Repository,
                                                                                  BuildEnv.TempFiles.CreateFile())
                                                                             .PipeTo(Self,
                                                                                  success:
                                                                                  c => new ContinuePushNewVersion(
                                                                                      OperationResult.Success(c),
                                                                                      m.Data.Event, reporter),
                                                                                  failure: e =>
                                                                                      new
                                                                                          ContinuePushNewVersion(
                                                                                              OperationResult
                                                                                                 .Failure(
                                                                                                      e.Unwrap()
                                                                                                        ?.Message ??
                                                                                                      "Cancel"),
                                                                                              m.Data.Event,
                                                                                              reporter))))
                                                                  ;
                                                          }));

            CommandPhase2<ContinuePushNewVersion, PushVersionCommand, AppBinary>("PushVersion-Phase 2",
                input => input
                        .Select(evt =>
                                {
                                    var (_, result, reporter, data, _) = evt;

                                    if (data != null)
                                        return !result.Ok ? null : evt;

                                    if (!reporter.IsCompled)
                                        reporter.Compled(OperationResult.Failure(BuildErrorCodes.CommandAppNotFound));
                                    return null;
                                }).NotNull()
                        .SelectMany(evt =>
                                        Observable.Using(async token =>
                                                             await evt.State.Apps.Database.Client.StartSessionAsync(
                                                                 new ClientSessionOptions
                                                                 {
                                                                     DefaultTransactionOptions =
                                                                         new TransactionOptions(
                                                                             writeConcern: WriteConcern.Acknowledged)
                                                                 }, token),
                                            (handle, _) => Task.FromResult(Observable.Return((evt, handle))))
                                                  )
                        .Select(_ => new AppBinary(0, "", DateTime.MinValue, true, ",", "")));

            CommandPhase2<ContinuePushNewVersion, PushVersionCommand, AppBinary>("PushVersion2",
                (command, result, reporter, data) =>
                {
                    using var transaction = apps.Database.Client.StartSession(new ClientSessionOptions
                                                                              {
                                                                                  DefaultTransactionOptions =
                                                                                      new TransactionOptions(
                                                                                          writeConcern: WriteConcern
                                                                                             .Acknowledged)
                                                                              });
                    var dataFilter = Builders<AppData>.Filter.Eq(ad => ad.Name, data.Name);

                    var (commit, fileName) = ((string, ITempFile)) result.Outcome!;

                    using var targetStream = fileName;

                    var newId = files.UploadFromStream(data.Name + ".zip", targetStream.Stream);

                    var newBinary = new AppFileInfo(newId, data.Last + 1, DateTime.UtcNow, false, commit);
                    var newBinarys = data.Versions.Add(newBinary);

                    var definition = Builders<AppData>.Update;
                    var updates = new List<UpdateDefinition<AppData>>
                                  {
                                      definition.Set(ad => ad.Last, newBinary.Version),
                                      definition.Set(ad => ad.Versions, newBinarys)
                                  };

                    var deleteUpdates = new List<ToDeleteRevision>();

                    if (data.Versions.Count(s => !s.Deleted) > 5)
                        foreach (var info in newBinarys.OrderByDescending(i => i.CreationTime).Skip(5))
                        {
                            if (info.Deleted) continue;
                            info.Deleted = true;
                            deleteUpdates.Add(new ToDeleteRevision(info.File.ToString()));
                        }

                    transaction.StartTransaction();

                    if (deleteUpdates.Count != 0)
                        toDelete.InsertMany(transaction, deleteUpdates);
                    if (!apps.UpdateOne(transaction, dataFilter, definition.Combine(updates)).IsAcknowledged)
                    {
                        transaction.AbortTransaction();
                        reporter.Compled(OperationResult.Failure(BuildErrorCodes.DatabaseError));
                        return null;
                    }

                    transaction.CommitTransaction();

                    changeTracker.Tell(_apps.AsQueryable().FirstOrDefault(ad => ad.Name == command.AppName));
                    return new AppBinary(command.AppName, newBinary.Version, newBinary.CreationTime, false,
                        newBinary.Commit, data.Repository);
                });
        }

        private void CommandPhase1<TCommand>(
            string name,
            Func<Reporter, IObservable<StatePair<TCommand, AppCommandProcessorState>>,
                IObservable<NewMessage<TCommand>>> executor,
            Func<TCommand, Reporter, IOperationResult, object> result)
            where TCommand : ReporterCommandBase<DeploymentApi, TCommand>, IDeploymentCommand
        {
            Receive<TCommand>(name, (reporter, observable) =>
                                    {
                                        IActorRef CreateListner(TCommand command)
                                        {
                                            return Reporter.CreateListner(Context, reporter, TimeSpan.FromSeconds(20),
                                                task => task.PipeTo(Self, Sender,
                                                    t => result(command, reporter, t),
                                                    e => result(command, reporter, OperationResult.Failure(e))));
                                        }

                                        return executor(reporter, observable)
                                              .ConditionalSelect()
                                              .ToResult<Unit>(s =>
                                                              {
                                                                  s.When(e => e.Message == null,
                                                                      obs => obs.ToUnit(r =>
                                                                      {
                                                                          if (!reporter.IsCompled)
                                                                              reporter.Compled(
                                                                                  OperationResult.Failure(
                                                                                      BuildErrorCodes
                                                                                         .GerneralCommandError));
                                                                          Log.Info(
                                                                              "Command Phase 1 {Command} Failed",
                                                                              typeof(TCommand).Name);
                                                                      }));

                                                                  s.When(e => e.Message != null,
                                                                      obs => obs.ToUnit(r =>
                                                                      {
                                                                          var (msg, command, state) = r;

                                                                          Log.Info(
                                                                              "Command Phase 1 {Command} -- {Action}",
                                                                              typeof(TCommand).Name,
                                                                              msg!.GetType().Name);

                                                                          msg.SetListner(CreateListner(command));
                                                                          ((ISender) state.Repository).SendCommand(
                                                                              msg);
                                                                      }));
                                                              });
                                    });
        }

        private void DirectCommandPhase1<TCommand>(
            string name,
            Func<Reporter, IObservable<StatePair<TCommand, AppCommandProcessorState>>, IObservable<Unit>> executor)
            where TCommand : ReporterCommandBase<DeploymentApi, TCommand>, IDeploymentCommand
            => Receive(name, executor);


        private void CommandPhase2<TContinue, TCommand, TResult>(
            string name,
            Func<IObservable<ContinueData<TCommand>>, IObservable<TResult?>> executor)
            where TContinue : ContinueCommand<TCommand>
            where TCommand : ReporterCommandBase<DeploymentApi, TCommand>, IDeploymentCommand
            where TResult : class
        {
            ReceiveContinue<TContinue>(name, (reporter, observable) =>
                                             {
                                                 return executor(observable.SelectMany(
                                                            async evt => new ContinueData<TCommand>(evt.Event.Command,
                                                                evt.Event.Result, evt.Event.Reporter,
                                                                await QueryApp(evt.State.Apps,
                                                                    evt.Event.Command.AppName),
                                                                evt.State)))
                                                       .Where(_ => !reporter.IsCompled)
                                                       .ToUnit(result =>
                                                               {
                                                                   if (Equals(result, default(TResult)) &&
                                                                       !reporter.IsCompled)
                                                                       reporter.Compled(
                                                                           OperationResult.Failure(BuildErrorCodes
                                                                              .GerneralCommandError));
                                                                   else
                                                                       reporter.Compled(
                                                                           OperationResult.Success(result));
                                                               });
                                             });
        }


        private static Task<AppData?> QueryApp(IMongoCollection<AppData> collection, string name)
            => collection.Find(Builders<AppData>.Filter.Where(ad => ad.Name == name)).FirstOrDefaultAsync()!;

        public sealed record AppCommandProcessorState(
            IMongoCollection<AppData> Apps, GridFSBucket Files,
            RepositoryApi Repository, DataTransferManager DataTransfer,
            IMongoCollection<ToDeleteRevision> ToDelete, IWorkDistributor<BuildRequest> WorkDistributor,
            IActorRef ChangeTracker);

        private static class Msg
        {
            public static NewMessage<TCommand> New<TCommand>(
                IReporterMessage? message,
                StatePair<TCommand, AppCommandProcessorState> pair)
                => new(message, pair.Event, pair.State);
        }

        private sealed record NewMessage<TCommand>(
            IReporterMessage? Message, TCommand Command,
            AppCommandProcessorState State);

        private sealed record ContinueData<TCommand>(
            TCommand Command, IOperationResult Result, Reporter Reporter,
            AppData? AppData, AppCommandProcessorState State)
            where TCommand : ReporterCommandBase<DeploymentApi, TCommand>, IDeploymentCommand;

        private abstract record ContinueCommand<TCommand>
            (IOperationResult Result, TCommand Command, Reporter Reporter) : IDelegatingMessage
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
        public AppCommandProcessor()
        {
            CommandPhase1<DeleteAppCommand>("DeleteApp", (command, reporter) =>
                                                         {
                                                             var data = apps.AsQueryable()
                                                                            .FirstOrDefault(
                                                                                 d => d.Name == command.AppName);
                                                             if (data == null)
                                                             {
                                                                 reporter.Compled(
                                                                     OperationResult.Failure(BuildErrorCodes
                                                                        .CommandAppNotFound));
                                                                 return;
                                                             }

                                                             var transaction = apps.Database.Client.StartSession();
                                                             transaction.StartTransaction();

                                                             var arr = data.Versions.Where(f => f.Deleted).ToArray();
                                                             if (arr.Length > 0)
                                                                 toDelete.InsertMany(transaction,
                                                                     arr.Select(
                                                                         f => new ToDeleteRevision(f.File.ToString())));
                                                             apps.DeleteOne(transaction,
                                                                 Builders<AppData>.Filter.Eq(a => a.Name, data.Name));

                                                             transaction.CommitTransaction();
                                                             reporter.Compled(
                                                                 OperationResult.Success(data.ToInfo().IsDeleted()));
                                                         });

            CommandPhase1<ForceBuildCommand>("ForceBuild", (command, reporter) =>
                                                           {
                                                               var tempData = new AppData(command.AppName, -1,
                                                                   DateTime.Now, DateTime.MinValue, command.Repository,
                                                                   command.Repository,
                                                                   ImmutableList<AppFileInfo>.Empty);
                                                               BuildRequest
                                                                  .SendWork(workDistributor, reporter, tempData,
                                                                       repository, BuildEnv.TempFiles.CreateFile())
                                                                  .PipeTo(Self,
                                                                       success: d => new ContinueForceBuild(
                                                                           OperationResult.Success(d.Item2),
                                                                           command, reporter),
                                                                       failure: e => new ContinueForceBuild(
                                                                           OperationResult.Failure(
                                                                               e.Unwrap()?.Message ?? "Cancel"),
                                                                           command, reporter));
                                                           });

            CommandPhase2<ContinueForceBuild, ForceBuildCommand, FileTransactionId>("ForceBuild2",
                (command, result, reporter, _) =>
                {
                    if (!result.Ok || command.Manager == null)
                        return null;

                    if (!(result.Outcome is TempStream target)) return null;

                    var request = DataTransferRequest.FromStream(target, command.Manager);
                    dataTransfer.Request(request);

                    return new FileTransactionId(request.OperationId);
                });
        }
    }
}
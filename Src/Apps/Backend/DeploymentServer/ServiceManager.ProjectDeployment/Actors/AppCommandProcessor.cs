﻿using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using Akka.Actor;
using Microsoft.Extensions.Logging;
using ServiceManager.ProjectDeployment.Build;
using ServiceManager.ProjectDeployment.Data;
using SharpRepository.Repository;
using Tauron;
using Tauron.Application;
using Tauron.Application.AkkaNode.Services.CleanUp;
using Tauron.Application.AkkaNode.Services.FileTransfer;
using Tauron.Application.AkkaNode.Services.Reporting;
using Tauron.Application.AkkaNode.Services.Reporting.Commands;
using Tauron.Application.Master.Commands;
using Tauron.Application.Master.Commands.Deployment;
using Tauron.Application.Master.Commands.Deployment.Build;
using Tauron.Application.Master.Commands.Deployment.Build.Commands;
using Tauron.Application.Master.Commands.Deployment.Build.Data;
using Tauron.Application.Master.Commands.Deployment.Repository;
using Tauron.Application.VirtualFiles;
using Tauron.Application.Workshop;
using Tauron.Features;
using Tauron.ObservableExt;
using Tauron.Operations;
using Tauron.Temp;
using UnitsNet;

namespace ServiceManager.ProjectDeployment.Actors
{
    public sealed class AppCommandProcessor : ReportingActor<AppCommandProcessor.AppCommandProcessorState>
    {
        public static IPreparedFeature New(
            IRepository<AppData, AppName> apps, IDirectory files, RepositoryApi repository,
            DataTransferManager dataTransfer, IRepository<ToDeleteRevision, string> toDelete, IWorkDistributor<BuildRequest> workDistributor,
            IActorRef changeTracker)
            => Feature.Create(
                () => new AppCommandProcessor(),
                _ => new AppCommandProcessorState(
                    apps,
                    files,
                    repository,
                    dataTransfer,
                    toDelete,
                    workDistributor,
                    changeTracker));

        protected override void ConfigImpl()
        {
            CommandPhase1<CreateAppCommand>(
                "CreateApp-Phase 1",
                obs => obs.Do(m => m.Reporter.Send(DeploymentMessage.RegisterRepository))
                   .Select(m => Msg.New(new RegisterRepository(m.Event.TargetRepo, IgnoreDuplicate: true), m)),
                (command, reporter, op) => new ContinueCreateApp(op, command, reporter));

            CommandPhase2<ContinueCreateApp, CreateAppCommand, AppInfo>(
                "CreateApp-Phase 2",
                CreateAppExecutor);

            DirectCommandPhase1<PushVersionCommand>(
                "PushVersion-Phase 1",
                PushVersionPhase1Executor);


            CommandPhase2<ContinuePushNewVersion, PushVersionCommand, AppBinary>(
                "PushVersion-Phase 2",
                PushVersionExecutor);

            DirectCommandPhase1<DeleteAppCommand>(
                "DeleteApp",
                DeleteExecutor);

            DirectCommandPhase1<ForceBuildCommand>(
                "ForceBuild-Phase 1",
                ForceBuildPhase1Executor);

            CommandPhase2<ContinueForceBuild, ForceBuildCommand, FileTransactionId>(
                "ForceBuild-Phase 2",
                ForceBuildExecutor,
                r => r.Event.TempData);
        }

        private IObservable<Unit> ForceBuildPhase1Executor(IObservable<ReporterEvent<ForceBuildCommand, AppCommandProcessorState>> obs)
        {
            return obs.Select(
                    i => i.New(
                        (Command: i.Event, App: new AppData(
                            ImmutableList<AppFileInfo>.Empty,
                            i.Event.AppName,
                            SimpleVersion.NoVersion,
                            DateTime.UtcNow,
                            DateTime.MinValue,
                            i.Event.Repository,
                            i.Event.Project))))
                .ToUnit(
                    i => BuildRequest.SendWork(
                            i.State.WorkDistributor,
                            i.Reporter,
                            i.Event.App,
                            i.State.Repository,
                            BuildEnv.TempFiles.CreateFile())
                        .PipeTo(
                            Self,
                            success: d => new ContinueForceBuild(OperationResult.Success(d.File), i.Event.Command, i.Reporter, i.Event.App),
                            failure: e => new ContinueForceBuild(OperationResult.Failure(e), i.Event.Command, i.Reporter, i.Event.App)));
        }

        private IObservable<Unit> PushVersionPhase1Executor(IObservable<ReporterEvent<PushVersionCommand, AppCommandProcessorState>> obs)
        {
            return obs.Select(m => (App: m.State.Apps.Get(m.Event.AppName), Data: m))
                .ConditionalSelect()
                .ToResult<Unit>(
                    b =>
                    {
                        b.When(
                            m => m.App is null,
                            o => o.ToUnit(d => d.Data.Reporter.Compled(OperationResult.Failure(DeploymentErrorCode.CommandAppNotFound))));

                        b.When(
                            m => m.App != null,
                            o => o.ToUnit(
                                d => BuildRequest.SendWork(
                                        d.Data.State.WorkDistributor,
                                        d.Data.Reporter,
                                        d.App,
                                        d.Data.State.Repository,
                                        BuildEnv.TempFiles.CreateFile())
                                    .PipeTo(
                                        Self,
                                        success: c => new ContinuePushNewVersion(OperationResult.Success(c), d.Data.Event, d.Data.Reporter),
                                        failure: e => new ContinuePushNewVersion(
                                            OperationResult.Failure(e),
                                            d.Data.Event,
                                            d.Data.Reporter))));
                    });
        }

        private static IObservable<Unit> DeleteExecutor(IObservable<ReporterEvent<DeleteAppCommand, AppCommandProcessorState>> obs)
        {
            return obs.Select(m => m.New((Command: m.Event, App: m.State.Apps.Get(m.Event.AppName))))
                .ConditionalSelect()
                .ToResult<Unit>(
                    b =>
                    {
                        b.When(
                            m => m.Event.App is null,
                            o => o.ToUnit(i => i.Reporter.Compled(OperationResult.Failure(DeploymentErrorCode.CommandAppNotFound))));

                        b.When(
                            m => m.Event.App is not null,
                            o => o.ToUnit(
                                i =>
                                {
                                    i.State.Apps.Delete(i.Event.App);
                                    i.State.ToDelete.Add(i.Event.App.Versions.Select(d => new ToDeleteRevision(d.Id)));

                                    i.Reporter.Compled(OperationResult.Success(i.Event.App.ToInfo().MarkDeleted()));
                                }));
                    });
        }

        private static IObservable<FileTransactionId?> ForceBuildExecutor(IObservable<ContinueData<ForceBuildCommand>> obs)
        {
            return obs.ConditionalSelect()
                .ToResult<FileTransactionId?>(
                    b =>
                    {
                        b.When(
                            m => m.Command.Manager is null || m.Result is { Ok: true, Outcome: not ITempFile },
                            o => o.Select(_ => default(FileTransactionId)));
                        b.When(
                            m => !m.Result.Ok,
                            o => o.ApplyWhen(d => !d.Reporter.IsCompled, data => data.Reporter.Compled(data.Result))
                                .Select(_ => default(FileTransactionId)));

                        b.When(
                            m => m is { Result: { Ok: true, Outcome: ITempFile }, Command.Manager: { } },
                            o => o.Select(
                                continueData =>
                                {
                                    var file = (ITempFile)continueData.Result.Outcome!;
                                    DataTransferManager man = continueData.Command.GetTransferManager();
                                    DataTransferManager start = continueData.State.DataTransfer;

                                    return start.RequestWithTransaction(DataTransferRequest.FromStream(() => new TempTransferInfo(file), man));
                                }));
                    });
        }


        private IObservable<AppBinary?> PushVersionExecutor(IObservable<ContinueData<PushVersionCommand>> obs) =>
                obs.ConditionalSelect()
                    .ToResult<AppBinary?>(
                        b =>
                        {
                            b.When(
                                m => m.AppData is null,
                                o => o.Do(m => m.Reporter.Compled(OperationResult.Failure(DeploymentErrorCode.CommandAppNotFound)))
                                    .Select(_ => default(AppBinary)));

                            b.When(m => m.AppData != null, o => o.SelectMany(UpdateAppData));

                            static IObservable<AppBinary?> UpdateAppData(ContinueData<PushVersionCommand> m)
                            {
                                (RepositoryCommit commit, ITempFile fileName) = ((RepositoryCommit, ITempFile))m.Result.Outcome!;

                                return Observable.Using(
                                    () => fileName,
                                    file => (from info in Observable.Return((Commit: commit, File: file, Data: m))
                                            let oldApp = info.Data.AppData ?? AppData.Empty
                                            let newVersion = oldApp.Last + 1
                                            let newId = $"{oldApp.Id}-{newVersion}.zip"
                                            let newBinary = new AppFileInfo(newId, oldApp.Last + 1, DateTime.UtcNow, Deleted: false, Commit: info.Commit)
                                            let newData = oldApp with { Last = newVersion, LastUpdate = DateTime.UtcNow, Versions = info.Data.AppData!.Versions.Add(newBinary) }
                                            select (info.Data.State, NewData: newData, NewBinary: newBinary, info.File, ToDelete: newData.Versions.OrderByDescending(i => i.CreationTime)
                                                .Skip(5)
                                                .Where(i => !i.Deleted)
                                                .ToArray())).Select(
                                            i =>
                                            {
                                                using Stream stream = i.State.Files.GetFile(i.NewBinary.Id).CreateNew();
                                                using Stream fileStream = i.File.Stream;

                                                fileStream.Seek(0, SeekOrigin.Begin);
                                                fileStream.CopyTo(stream);

                                                AppData newData = i.ToDelete.Aggregate(
                                                    i.NewData,
                                                    (current, appFileInfo) => current! with
                                                    {
                                                        Versions = current.Versions.Replace(
                                                            appFileInfo,
                                                            appFileInfo with { Deleted = true })
                                                    });

                                                i.State.Apps.Update(newData!);
                                                i.State.ToDelete.Add(i.ToDelete.Select(e => new ToDeleteRevision(e.Id)));

                                                return (Data: newData, i.NewBinary, i.State);
                                            })
                                        .Do(i => i.State.ChangeTracker.Tell(i.Data!.ToInfo()))
                                        .Select(
                                            i => new AppBinary(
                                                i.NewBinary.Version,
                                                i.Data!.Id,
                                                i.NewBinary.CreationTime,
                                                Deleted: false,
                                                i.NewBinary.Commit,
                                                i.Data.Repository)));
                            }
                        });
        
        private static IObservable<AppInfo?> CreateAppExecutor(IObservable<ContinueData<CreateAppCommand>> obs)
        {
            return obs.ConditionalSelect()
                .ToResult<AppInfo?>(
                    b =>
                    {
                        b.When(
                            m => !m.Result.Ok,
                            o => o.ApplyWhen(
                                    m => !m.Reporter.IsCompled,
                                    data =>
                                    {
                                        data.Reporter.Compled(
                                            OperationResult.Failure(
                                                data.Result,
                                                DeploymentErrorCode.CommandErrorRegisterRepository));
                                    })
                                .Select(_ => default(AppInfo)));

                        b.When(
                            m => m.AppData != null,
                            o => o.Do(m => m.Reporter.Compled(OperationResult.Failure(DeploymentErrorCode.CommandDuplicateApp.ToString())))
                                .Select(_ => default(AppInfo)));

                        b.When(
                            m => m.AppData is null,
                            o => o.Select(
                                    m =>
                                    {
                                        var data = new AppData(
                                            ImmutableList<AppFileInfo>.Empty,
                                            m.Command.AppName,
                                            SimpleVersion.NoVersion,
                                            DateTime.UtcNow,
                                            DateTime.MinValue,
                                            m.Command.TargetRepo,
                                            m.Command.ProjectName);
                                        m.State.Apps.Add(data);

                                        return (Data: data.ToInfo(), Tracker: m.State.ChangeTracker);
                                    })
                                .Do(i => i.Tracker.Tell(i.Data))
                                .Select(i => i.Data));
                    });
        }

        private void CommandPhase1<TCommand>(
            string name,
            Func<IObservable<ReporterEvent<TCommand, AppCommandProcessorState>>, IObservable<NewMessage<TCommand>>> executor,
            Func<TCommand, Reporter, IOperationResult, object> result)
            where TCommand : ReporterCommandBase<DeploymentApi, TCommand>, IDeploymentCommand
        {
            var logger = TauronEnviroment.LoggerFactory.CreateLogger<AppCommandProcessor>();
            
            TryReceive<TCommand>(
                name,
                obs => obs.SelectMany(
                    reporterEvent => executor(Observable.Return(reporterEvent))
                       .ConditionalSelect()
                       .ToResult<Unit>(
                            b =>
                            {
                                b.When(
                                    m => m.Message is null,
                                    o => o.ToUnit(
                                        _ =>
                                        {
                                            if (reporterEvent.Reporter.IsCompled)
                                                reporterEvent.Reporter.Compled(OperationResult.Failure(DeploymentErrorCode.GerneralCommandError));
                                            logger.LogInformation("Command Phase 1 {Command} Failed", typeof(TCommand).Name);
                                        }));

                                b.When(
                                    m => m.Message is not null,
                                    o => o.ToUnit(
                                        r =>
                                        {
                                            (IReporterMessage? msg, TCommand command, AppCommandProcessorState state) = r;
                                            logger.LogInformation("Command Phase 1 {Command} -- {Action}", typeof(TCommand).Name, msg!.GetType().Name);

                                            msg.SetListner(
                                                Reporter.CreateListner(
                                                    Context,
                                                    reporterEvent.Reporter,
                                                    Duration.FromSeconds(20), 
                                                    task => task.PipeTo(
                                                        Self,
                                                        Sender,
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

            TryContinue<TContinue>(
                name,
                obs => obs.SelectMany(
                    evt => executor(
                            Observable.Return(
                                new ContinueData<TCommand>(evt.Event.Command, evt.Event.Result, evt.Reporter, queryApp(evt), evt.State)))
                       .Where(_ => !evt.Reporter.IsCompled)
                       .ToUnit(result => evt.Reporter.Compled(result is null ? OperationResult.Failure(DeploymentErrorCode.GerneralCommandError) : OperationResult.Success(result)))));
        }


        private static AppData QueryApp(ICrudRepository<AppData, AppName> collection, in AppName name)
            => collection.Get(name);

        public sealed record AppCommandProcessorState(
            IRepository<AppData, AppName> Apps, IDirectory Files,
            RepositoryApi Repository, DataTransferManager DataTransfer,
            IRepository<ToDeleteRevision, string> ToDelete, IWorkDistributor<BuildRequest> WorkDistributor,
            IActorRef ChangeTracker);

        private static class Msg
        {
            internal static NewMessage<TCommand> New<TCommand>(
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
            internal ContinueCreateApp(IOperationResult result, CreateAppCommand command, Reporter reporter)
                : base(result, command, reporter) { }
        }

        private sealed record ContinuePushNewVersion : ContinueCommand<PushVersionCommand>
        {
            // ReSharper disable once NotAccessedField.Local
            private readonly Reporter _reporter;

            internal ContinuePushNewVersion(
                IOperationResult result, PushVersionCommand command,
                Reporter reporter)
                : base(result, command, reporter)
                => _reporter = reporter;
        }

        private sealed record ContinueForceBuild : ContinueCommand<ForceBuildCommand>
        {
            internal ContinueForceBuild(
                IOperationResult result, ForceBuildCommand command,
                Reporter reporter, AppData tempData) : base(result, command, reporter)
                => TempData = tempData;

            internal AppData TempData { get; }
        }

        private sealed class TempTransferInfo : ITransferData
        {
            private readonly ITempFile _file;
            private readonly Stream _stream;

            internal TempTransferInfo(ITempFile file)
            {
                _file = file;
                _stream = file.Stream;
                _stream.Seek(0, SeekOrigin.Begin);
            }

            public void Dispose()
            {
                _stream.Dispose();
                _file.Dispose();
            }

            public int Read(byte[] buffer, in int offset, in int count) => _stream.Read(buffer, offset, count);

            public void Write(byte[] buffer, in int offset, in int count) => _stream.Write(buffer, offset, count);
        }
    }
}
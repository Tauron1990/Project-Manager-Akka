using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using Akka.Actor;
using ServiceManager.ProjectDeployment.Data;
using SharpRepository.Repository;
using Tauron;
using Tauron.Application.AkkaNode.Services;
using Tauron.Application.AkkaNode.Services.FileTransfer;
using Tauron.Application.AkkaNode.Services.Reporting;
using Tauron.Application.AkkaNode.Services.Reporting.Commands;
using Tauron.Application.Files.VirtualFiles;
using Tauron.Application.Master.Commands.Deployment.Build;
using Tauron.Application.Master.Commands.Deployment.Build.Data;
using Tauron.Application.Master.Commands.Deployment.Build.Querys;
using Tauron.Features;
using Tauron.ObservableExt;
using Tauron.Operations;

namespace ServiceManager.ProjectDeployment.Actors
{
    public sealed class AppQueryHandler : ReportingActor<AppQueryHandler.AppQueryHandlerState>
    {
        public static IPreparedFeature New(IRepository<AppData, string> apps, IDirectory files, DataTransferManager dataTransfer, IActorRef changeTracker)
            => Feature.Create(() => new AppQueryHandler(), _ => new AppQueryHandlerState(apps, files, dataTransfer, changeTracker));
        
        public sealed record AppQueryHandlerState(IRepository<AppData, string> Apps, IDirectory Files, DataTransferManager DataTransfer, IActorRef ChangeTracker);

        protected override void ConfigImpl()
        {
            Receive<QueryAppChangeSource>(obs => obs.Select(e => e.Event).ForwardToActor(CurrentState.ChangeTracker));

            MakeQueryCall<QueryApps, AppList>("QueryApps",
                obs => obs.Select(evt => evt.New<AppList?>(new AppList(evt.State.Apps.GetAll().Select(d => d.ToInfo()).ToImmutableList()))));

            MakeQueryCall<QueryApp, AppInfo>("QueryApp",
                obs => obs.Select(m => m.New(m.State.Apps.Get(m.Event.AppName)))
                          .ConditionalSelect()
                          .ToResult<ReporterEvent<AppInfo?, AppQueryHandlerState>>(
                               b =>
                               {
                                   b.When(m => m.Event == null, o => o.Select(m => m.New(default(AppInfo))
                                                                                    .CompledReporter(OperationResult.Failure(BuildErrorCodes.QueryAppNotFound))));
                                   b.When(m => m.Event != null, o => o.Select(evt => evt.New<AppInfo?>(evt.Event.ToInfo())));
                               }));

            MakeQueryCall<QueryBinarys, FileTransactionId>("QueryBinaries",
                obs => obs.Select(m => m.New((App:m.State.Apps.Get(m.Event.AppName), Query:m.Event)))
                          .ConditionalSelect()
                          .ToResult<ReporterEvent<FileTransactionId?, AppQueryHandlerState>>(
                               b =>
                               {
                                   b.When(m => m.Event.Query.Manager == null, o => o.Select(evt => evt.New(default(FileTransactionId))));
                                   b.When(m => m.Event.App == null, o => o.Select(evt => evt
                                                                                        .New(default(FileTransactionId))
                                                                                        .CompledReporter(OperationResult.Failure(BuildErrorCodes.QueryAppNotFound))));
                                   b.When(d => d.Event.App != null, SelectVersion);

                                   static IObservable<ReporterEvent<FileTransactionId?, AppQueryHandlerState>> SelectVersion(
                                       IObservable<ReporterEvent<(AppData App, QueryBinarys Query), AppQueryHandlerState>> obs)
                                       => obs.Select(m => m.New((m.Event.Query, m.Event.App, TargetVersion: m.Event.Query.AppVersion == -1 ? m.Event.App.Last : m.Event.Query.AppVersion)))
                                             .Select(m => m.New((m.Event.Query, m.Event.App, File:m.Event.App.Versions.FirstOrDefault(fi => fi.Version == m.Event.TargetVersion))))
                                             .ConditionalSelect()
                                             .ToResult<ReporterEvent<FileTransactionId?, AppQueryHandlerState>>(
                                                  b =>
                                                  {
                                                      b.When(m => m.Event.File == null,
                                                          o => o.Select(evt => evt
                                                                              .New(default(FileTransactionId))
                                                                              .CompledReporter(OperationResult.Failure(BuildErrorCodes.QueryFileNotFound))));
                                                      b.When(m => m.Event.File != null,
                                                          o => o
                                                              .Select(evt => evt.New((
                                                                          Manager: evt.Event.Query.GetTransferManager(),
                                                                          File: evt.State.Files.GetFile(evt.Event.File!.Id),
                                                                          evt.Event.Query.AppName)))
                                                              .Select(evt => evt.New(DataTransferRequest.FromStream(
                                                                          () => evt.Event.File.Open(FileAccess.Read),
                                                                          evt.Event.Manager,
                                                                          evt.Event.AppName)))
                                                              .Select(evt => evt.New<FileTransactionId?>(evt.State.DataTransfer.Request(evt.Event))));
                                                  });
                               }));

            MakeQueryCall<QueryBinaryInfo, BinaryList>("QueryBinaryInfo",
                obs => obs.Select(m => m.New(m.State.Apps.Get(m.Event.AppName)))
                          .ConditionalSelect()
                          .ToResult<ReporterEvent<BinaryList?, AppQueryHandlerState>>(
                               b =>
                               {
                                   b.When(evt => evt.Event == null, o => o.Select(evt => evt
                                                                                        .New(default(BinaryList))
                                                                                        .CompledReporter(OperationResult.Failure(BuildErrorCodes.QueryAppNotFound))));
                                   b.When(evt => evt.Event != null,
                                       o => o.Select(d => d.New<BinaryList?>(
                                                         new BinaryList(d.Event.Versions
                                                                         .Select(i => new AppBinary(i.Version, d.Event.Id, i.CreationTime, i.Deleted, i.Commit, d.Event.Repository))
                                                                         .ToImmutableList()))));
                               }));
        }

        private void MakeQueryCall<T, TResult>(string name, Func<IObservable<ReporterEvent<T, AppQueryHandlerState>>, IObservable<ReporterEvent<TResult?, AppQueryHandlerState>>> handler) 
            where TResult : class 
            where T : IReporterMessage
            => TryReceive<T>(name,
                obs => handler(obs)
                   .ToUnit(m =>
                           {
                               if(m.Reporter.IsCompled) return;
                               m.Reporter.Compled(m.Event == default
                               ? OperationResult.Failure(BuildErrorCodes.GeneralQueryFailed)
                               : OperationResult.Success(m.Event));
                           }));
    }
}
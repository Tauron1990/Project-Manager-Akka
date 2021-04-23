using System;
using System.Reactive.Linq;
using Octokit;
using ServiceManager.ProjectRepository.Core;
using ServiceManager.ProjectRepository.Data;
using SharpRepository.Repository;
using Tauron;
using Tauron.Akka;
using Tauron.Application.AkkaNode.Services;
using Tauron.Application.AkkaNode.Services.CleanUp;
using Tauron.Application.AkkaNode.Services.FileTransfer;
using Tauron.Application.Files.VirtualFiles;
using Tauron.Application.Master.Commands.Deployment.Repository;
using Tauron.Features;
using Tauron.Operations;
using Tauron.Temp;

using RequestResult = Tauron.Application.AkkaNode.Services.ReporterEvent<Tauron.Operations.IOperationResult, ServiceManager.ProjectRepository.Actors.OperatorActor.OperatorState>;
using RegisterRepoEvent = Tauron.Application.AkkaNode.Services.ReporterEvent<Tauron.Application.Master.Commands.Deployment.Repository.RegisterRepository, ServiceManager.ProjectRepository.Actors.OperatorActor.OperatorState>;
using TransferRepositoryRequest = Tauron.Application.AkkaNode.Services.ReporterEvent<Tauron.Application.Master.Commands.Deployment.Repository.TransferRepository, ServiceManager.ProjectRepository.Actors.OperatorActor.OperatorState>;

namespace ServiceManager.ProjectRepository.Actors
{
    public sealed class OperatorActor : ReportingActor<OperatorActor.OperatorState>
    {
        public static IPreparedFeature New(
            IRepository<RepositoryEntry, string> repos, IVirtualFileSystem bucket,
            IRepository<ToDeleteRevision, string> revisions, DataTransferManager dataTransfer)
            => Feature.Create(() => new OperatorActor(), c => new OperatorState(repos, bucket, revisions, dataTransfer,
                new GitHubClient(new ProductHeaderValue(c.System.Settings.Config.GetString("akka.appinfo.applicationName", "Test Apps").Replace(' ', '_')))));

        protected override void ConfigImpl()
        {
            TryReceive<RegisterRepository>("RegisterRepository",
                obs => obs
                      .Do(i => Log.Info("Incomming Registration Request for Repository {Name}", i.Event.RepoName))
                      .Do(i => i.Reporter.Send(RepositoryMessages.GetRepo))
                      .Select(i => (Data: i.State.Repos.Get(i.Event.RepoName), Evt: i))
                      .SelectMany(ProcessRegisterRepository)
                      .NotNull()
                      .ObserveOnSelf()
                      .Finally(() => Context.Stop(Self))
                      .ToUnit(r => r.Reporter.Compled(r.Event)));

            TryReceive<TransferRepository>("RequestRepository",
                obs => obs
                      .Do(i => Log.Info("Incomming Transfer Request for Repository {Name}", i.Event.RepoName))
                      .SelectMany(ProcessRequestRepository)
                      .NotNull()
                      .ObserveOnSelf()
                      .ApplyWhen(d => !d.Event.Ok, _ => Context.Stop(Self))
                      .ToUnit(r => r.Reporter.Compled(r.Event)));

            Receive<ITempFile>(
                obs => obs
                      .Select(e => e.Event)
                      .SubscribeWithStatus(c =>
                                           {
                                               c.Dispose();
                                               Context.Stop(Self);
                                           }));
        }

        private IObservable<RequestResult> ProcessRegisterRepository((RepositoryEntry Data, RegisterRepoEvent Evt) d)
        {
            return Observable.If
            (
                () => d.Data != null,
                Observable.Return(d.Evt)
                          .Do(i => Log.Info("Repository {Name} is Registrated", i.Event.RepoName))
                          .SelectMany(
                               m => Observable.If(
                                   () => m.Event.IgnoreDuplicate,
                                   ObservableReturn(() => m.New(OperationResult.Success())),
                                   ObservableReturn(() => m.New(OperationResult.Failure(RepoErrorCodes.DuplicateRepository))))),
                ValidateName(Observable.Return(d), CreateRepo)
            );

            IObservable<RequestResult> ValidateName(
                IObservable<(RepositoryEntry Data, RegisterRepoEvent Evt)> input,
                Func<IObservable<RegisterRepoEvent>, IObservable<RequestResult>> next)
                => input.SelectMany(
                    m => Observable.If(
                        () => !m.Data.RepoName.Contains('/'),
                        ObservableReturn(() => m.Evt.New(OperationResult.Failure(RepoErrorCodes.InvalidRepoName)))
                           .Do(_ => Log.Info("Repository {Name} Name is Invalid", m.Evt.Event.RepoName)),
                        next(Observable.Return(m.Evt))));

            IObservable<RequestResult> CreateRepo(IObservable<RegisterRepoEvent> request)
                => request
                  .Select(m => (Evt:m, NameSplit:m.Event.RepoName.Split('/')))
                  .SelectMany(async m => (m.Evt, Repo: await m.Evt.State.GitHubClient.Repository.Get(m.NameSplit[0], m.NameSplit[1])))
                   .SelectMany(
                       m => Observable.If(
                           () => m.Repo == null,
                       Observable.Return(m.Evt)
                                 .Do(evt => Log.Info("Repository {Name} Name not found on Github", evt.Event.RepoName))
                                 .Select(evt => evt.New(OperationResult.Failure(RepoErrorCodes.GithubNoRepoFound))),
                           SaveRepo(Observable.Return(m))));

            static IObservable<RequestResult> SaveRepo(IObservable<(RegisterRepoEvent Request, Repository Repo)> input)
                => input.Select(m => (m.Request, Data: new RepositoryEntry(m.Request.Event.RepoName, string.Empty, m.Repo.CloneUrl, m.Request.Event.RepoName, string.Empty, false, m.Repo.Id)))
                        .Select(m =>
                                {
                                    var (request, data) = m;
                                    request.State.Repos.Add(data);
                                    return request;
                                })
                        .Select(m => m.New(OperationResult.Success()));
        }

        private IObservable<RequestResult> ProcessRequestRepository(TransferRepositoryRequest request)
        {
            return Observable.Return(request)
                             .Select(m => (TempFiles: RepoEnv.TempFiles.CreateFile(), Request: m))
                             .Do(m => m.Request.Reporter.Send(RepositoryMessages.GetRepo))
                             .Select(m => (m.TempFiles, m.Request, Data: m.Request.State.Repos.Get(m.Request.Event.RepoName)))
                             .SelectMany(CheckData);

            IObservable<RequestResult> CheckData((ITempFile TempFiles, TransferRepositoryRequest Request, RepositoryEntry Data) input)
                => Observable.If(
                    () => input.Data == null,
                    Observable.Return(input.Request)
                              .Select(m => m.New(OperationResult.Failure(RepoErrorCodes.DatabaseNoRepoFound))));
        }

        private static IObservable<TData> ObservableReturn<TData>(Func<TData> fac)
            => Observable.Defer(() => Observable.Return(fac()));

        public sealed record OperatorState(IRepository<RepositoryEntry, string> Repos, IVirtualFileSystem Bucket,
            IRepository<ToDeleteRevision, string> Revisions, DataTransferManager DataTransferManager, GitHubClient GitHubClient);
    }
}
using System;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using Akka.Actor;
using Ionic.Zip;
using Octokit;
using ServiceManager.ProjectRepository.Core;
using ServiceManager.ProjectRepository.Data;
using SharpRepository.Repository;
using Tauron;
using Tauron.Akka;
using Tauron.Application.AkkaNode.Services;
using Tauron.Application.AkkaNode.Services.FileTransfer;
using Tauron.Application.AkkaNode.Services.Reporting;
using Tauron.Application.AkkaNode.Services.Reporting.Commands;
using Tauron.Application.Files.VirtualFiles;
using Tauron.Application.Master.Commands.Deployment.Repository;
using Tauron.Features;
using Tauron.Operations;
using Tauron.Temp;

using RequestResult = Tauron.Application.AkkaNode.Services.Reporting.ReporterEvent<Tauron.Operations.IOperationResult, ServiceManager.ProjectRepository.Actors.OperatorActor.OperatorState>;
using RegisterRepoEvent = Tauron.Application.AkkaNode.Services.Reporting.ReporterEvent<Tauron.Application.Master.Commands.Deployment.Repository.RegisterRepository, ServiceManager.ProjectRepository.Actors.OperatorActor.OperatorState>;
using Repository = Octokit.Repository;
using TransferRepositoryRequest = Tauron.Application.AkkaNode.Services.Reporting.ReporterEvent<Tauron.Application.Master.Commands.Deployment.Repository.TransferRepository, ServiceManager.ProjectRepository.Actors.OperatorActor.OperatorState>;

namespace ServiceManager.ProjectRepository.Actors
{
    public sealed class OperatorActor : ReportingActor<OperatorActor.OperatorState>
    {
        public static IPreparedFeature New(IRepository<RepositoryEntry, string> repos, IDirectory bucket, DataTransferManager dataTransfer)
            => Feature.Create(() => new OperatorActor(), c => new OperatorState(repos, bucket, dataTransfer,
                new GitHubClient(new ProductHeaderValue(c.System.Settings.Config.GetString("akka.appinfo.applicationName", "Test Apps").Replace(' ', '_')))));

        protected override void ConfigImpl()
        {
            Receive<Status>().Subscribe();

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
                        () => !m.Evt.Event.RepoName.Contains('/'),
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
                              .Select(m => m.New(OperationResult.Failure(RepoErrorCodes.DatabaseNoRepoFound))),
                    GetData(Observable.Return(input)));

            IObservable<RequestResult> GetData(IObservable<(ITempFile TempFiles, TransferRepositoryRequest Request, RepositoryEntry Data)> input)
                => input.SelectMany(async m => (m.Request, m.Data, m.TempFiles,
                                                CommitInfo: await m.Request.State.GitHubClient.Repository.Commit.GetSha1(m.Data.RepoId, "HEAD"), RepoZip: m.TempFiles.Stream))
                        .ApplyWhen(i => !(i.CommitInfo != i.Data.LastUpdate && UpdateRepository(i.Data, i.Request.Reporter, i.Request.Event, i.CommitInfo, i.RepoZip, i.Request.State)),
                             i =>
                             {
                                 i.Request.Reporter.Send(RepositoryMessages.GetRepositoryFromDatabase);
                                 Log.Info("Downloading Repository {Name} From Server", i.Request.Event.RepoName);
                                 i.RepoZip.SetLength(0);
                                 var dataStream = i.Request.State.Bucket.GetFile(i.Data.FileName).Open(FileAccess.Read);
                                 dataStream.CopyTo(i.RepoZip);
                                 i.RepoZip.Seek(0, SeekOrigin.Begin);
                             })
                        .Select(r =>
                                {
                                    var transferRequest = DataTransferRequest.FromStream(r.RepoZip, r.Request.Event.GetTransferManager(), r.CommitInfo);
                                    r.Request.State.DataTransferManager.Request(transferRequest.Inform(r.TempFiles));

                                    return (r.Request, Transfer: transferRequest);
                                })
                        .Select(r => r.Request.New(OperationResult.Success(new FileTransactionId(r.Transfer.OperationId))));
        }

        private bool UpdateRepository(RepositoryEntry data, Reporter reporter, TransferRepository repository, string commitInfo, Stream repozip, OperatorState state)
        {
            var (repos, bucket, _, _) = state;

            Log.Info("Try Update Repository");
            var downloadCompled = false;
            var repoConfiguration = new RepositoryConfiguration(data.SourceUrl, reporter);
            using var repoPath = RepoEnv.TempFiles.CreateDic();

            var repoName = repository.RepoName;
            var data2 = repos.AsQueryable().FirstOrDefault(r => r.RepoName == repoName);
            if (data2 != null && commitInfo != data2.LastUpdate)
            {
                if (!string.IsNullOrWhiteSpace(data.FileName))
                    try
                    {
                        Log.Info("Downloading Repository {Name} From Server", repoName);

                        reporter.Send(RepositoryMessages.GetRepositoryFromDatabase);
                        using var file = bucket.GetFile(data.FileName).Open(FileAccess.Read);
                        file.CopyTo(repozip);
                        
                        downloadCompled = true;
                    }
                    catch (Exception e)
                    {
                        Log.Error(e, "Error on Download Repo File {Name}", data.FileName);
                    }

                if (downloadCompled)
                {
                    Log.Info("Unpack Repository {Name}", repoName);
                    
                    repozip.Seek(0, SeekOrigin.Begin);
                    using var unpackZip = ZipFile.Read(repozip);

                    reporter.Send(RepositoryMessages.ExtractRepository);
                    unpackZip.ExtractAll(repoPath.FullPath, ExtractExistingFileAction.OverwriteSilently);
                }

                Log.Info("Execute Git Pull for {Name}", repoName);
                using var updater = GitUpdater.GetOrNew(repoConfiguration);
                var result = updater.RunUpdate(repoPath.FullPath);
                var dataUpdate = data with{ LastUpdate = result.Sha};

                Log.Info("Compress Repository {Name}", repoName);
                reporter.Send(RepositoryMessages.CompressRepository);

                if (repozip.Length != 0)
                    repozip.SetLength(0);

                using (var archive = new ZipFile())
                {
                    archive.AddDirectory(repoPath.FullPath, "");
                    archive.Save(repozip);
                }

                repozip.Seek(0, SeekOrigin.Begin);

                Log.Info("Upload and Update Repository {Name}", repoName);
                reporter.Send(RepositoryMessages.UploadRepositoryToDatabase);
                var id = repoName.Replace('/', '_') + ".zip";
                using var newFile = bucket.GetFile(id).Create();
                repozip.CopyTo(newFile);

                dataUpdate = dataUpdate with{FileName = id};

                repos.Update(dataUpdate);
                repozip.Seek(0, SeekOrigin.Begin);

                return true;
            }

            return false;
        }

        private static IObservable<TData> ObservableReturn<TData>(Func<TData> fac)
            => Observable.Defer(() => Observable.Return(fac()));

        public sealed record OperatorState(IRepository<RepositoryEntry, string> Repos, IDirectory Bucket, DataTransferManager DataTransferManager, GitHubClient GitHubClient);
    }
}
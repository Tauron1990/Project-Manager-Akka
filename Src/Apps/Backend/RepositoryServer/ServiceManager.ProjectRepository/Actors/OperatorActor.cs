using System;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using Akka.Actor;
using Akka.Streams.Implementation.Fusing;
using Ionic.Zip;
using Microsoft.Extensions.Logging;
using Octokit;
using ServiceManager.ProjectRepository.Core;
using ServiceManager.ProjectRepository.Data;
using SharpRepository.Repository;
using SharpRepository.Repository.Traits;
using Tauron;
using Tauron.Application;
using Tauron.Application.AkkaNode.Services.FileTransfer;
using Tauron.Application.AkkaNode.Services.Reporting;
using Tauron.Application.AkkaNode.Services.Reporting.Commands;
using Tauron.Application.Master.Commands.Deployment;
using Tauron.Application.Master.Commands.Deployment.Repository;
using Tauron.Application.VirtualFiles;
using Tauron.Features;
using Tauron.Operations;
using Tauron.TAkka;
using Tauron.Temp;
using RequestResult = Tauron.Application.AkkaNode.Services.Reporting.ReporterEvent<Tauron.Operations.IOperationResult, ServiceManager.ProjectRepository.Actors.OperatorActor.OperatorState>;
using RegisterRepoEvent = Tauron.Application.AkkaNode.Services.Reporting.ReporterEvent<Tauron.Application.Master.Commands.Deployment.Repository.RegisterRepository, ServiceManager.ProjectRepository.Actors.OperatorActor.OperatorState>;
using TransferRepositoryRequest = Tauron.Application.AkkaNode.Services.Reporting.ReporterEvent<Tauron.Application.Master.Commands.Deployment.Repository.TransferRepository, ServiceManager.ProjectRepository.Actors.OperatorActor.OperatorState>;

namespace ServiceManager.ProjectRepository.Actors
{
    public sealed class OperatorActor : ReportingActor<OperatorActor.OperatorState>
    {
        public static IPreparedFeature New(IRepository<RepositoryEntry, RepositoryName> repos, IDirectory bucket, DataTransferManager dataTransfer)
            => Feature.Create(
                () => new OperatorActor(),
                c => new OperatorState(
                    repos,
                    bucket,
                    dataTransfer,
                    new GitHubClient(new ProductHeaderValue(c.System.Settings.Config.GetString("akka.appinfo.applicationName", "Test Apps").Replace(' ', '_')))));

        protected override void ConfigImpl()
        {
            Receive<Status>().Subscribe();

            TryReceive<RegisterRepository>(
                "RegisterRepository",
                obs => obs
                   .Do(i => Logger.LogInformation("Incomming Registration Request for Repository {Name}", i.Event.RepoName))
                   .Do(i => i.Reporter.Send(RepositoryMessage.GetRepo))
                   .Select(i => (Data: i.State.Repos.Get(i.Event.RepoName), Evt: i))
                   .SelectMany(ProcessRegisterRepository)
                   .NotNull()
                   .ObserveOnSelf()
                   .Finally(() => Context.Stop(Self))
                   .ToUnit(r => r.Reporter.Compled(r.Event)));

            TryReceive<TransferRepository>(
                "RequestRepository",
                obs => obs
                   .Do(i => Logger.LogInformation("Incomming Transfer Request for Repository {Name}", i.Event.RepoName))
                   .SelectMany(ProcessRequestRepository)
                   .NotNull()
                   .ObserveOnSelf()
                   .ApplyWhen(d => !d.Event.Ok, _ => Context.Stop(Self))
                   .ToUnit(r => r.Reporter.Compled(r.Event)));

            Receive<ITempFile>(
                obs => obs
                   .Select(e => e.Event)
                   .SubscribeWithStatus(
                        c =>
                        {
                            c.Dispose();
                            Context.Stop(Self);
                        }));
        }

        private IObservable<RequestResult> ProcessRegisterRepository((RepositoryEntry? Data, RegisterRepoEvent Evt) d)
        {
            return Observable.If(
                () => d.Data is null,
                Observable.Return(d.Evt)
                   .Do(i => Logger.LogInformation("Repository {Name} is Registrated", i.Event.RepoName))
                   .SelectMany(
                        m => Observable.If(
                            () => m.Event.IgnoreDuplicate,
                            ObservableReturn(() => m.New(OperationResult.Success())),
                            ObservableReturn(() => m.New(OperationResult.Failure(RepositoryErrorCode.DuplicateRepository))))),
                ValidateName(Observable.Return(d)!, CreateRepo)
            );

            IObservable<RequestResult> ValidateName(
                IObservable<(RepositoryEntry Data, RegisterRepoEvent Evt)> input,
                Func<IObservable<RegisterRepoEvent>, IObservable<RequestResult>> next)
                => input.SelectMany(
                    m => Observable.If(
                        () => !m.Evt.Event.RepoName.IsValid,
                        ObservableReturn(() => m.Evt.New(OperationResult.Failure(RepositoryErrorCode.InvalidRepoName)))
                           .Do(_ => Logger.LogInformation("Repository {Name} Name is Invalid", m.Evt.Event.RepoName)),
                        next(Observable.Return(m.Evt))));

            IObservable<RequestResult> CreateRepo(IObservable<RegisterRepoEvent> request)
                => request
                   .Select(m => (Evt: m, NameSplit: m.Event.RepoName.GetSegments()))
                   .SelectMany(async m => (m.Evt, Repo: await m.Evt.State.GitHubClient.Repository.Get(m.NameSplit[0], m.NameSplit[1])))
                   .SelectMany(
                        m => Observable.If(
                            () => m.Repo == null,
                            Observable.Return(m.Evt)
                               .Do(evt => Logger.LogInformation("Repository {Name} Name not found on Github", evt.Event.RepoName))
                               .Select(evt => evt.New(OperationResult.Failure(RepositoryErrorCode.GithubNoRepoFound))),
                            SaveRepo(Observable.Return(m))));

            static IObservable<RequestResult> SaveRepo(IObservable<(RegisterRepoEvent Request, Repository Repo)> input)
                => input.Select(m => (m.Request, Data: new RepositoryEntry(m.Request.Event.RepoName, string.Empty, m.Repo.CloneUrl, m.Request.Event.RepoName, string.Empty, IsUploaded: false, m.Repo.Id)))
                   .Select(
                        m =>
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
               .Do(m => m.Request.Reporter.Send(RepositoryMessage.GetRepo))
               .Select(m => (m.TempFiles, m.Request, Data: m.Request.State.Repos.Get(m.Request.Event.RepoName)))
               .SelectMany(CheckData);

            IObservable<RequestResult> CheckData((ITempFile TempFiles, TransferRepositoryRequest Request, RepositoryEntry? Data) input)
                => Observable.If(
                    () => input.Data is null,
                    Observable.Return(input.Request)
                       .Select(m => m.New(OperationResult.Failure(RepositoryErrorCode.DatabaseNoRepoFound))),
                    GetData(Observable.Return(input)!));

            IObservable<RequestResult> GetData(IObservable<(ITempFile TempFiles, TransferRepositoryRequest Request, RepositoryEntry Data)> input)
                => input.SelectMany(
                        async m => (m.Request, m.Data, m.TempFiles,
                                    CommitInfo: await m.Request.State.GitHubClient.Repository.Commit.GetSha1(m.Data.RepoId, "HEAD").ConfigureAwait(false), RepoZip: m.TempFiles.Stream))
                   .ApplyWhen(
                        i => !(!string.Equals(i.CommitInfo, i.Data.LastUpdate, StringComparison.Ordinal) 
                               && UpdateRepository(i.Data, i.Request.Reporter, i.Request.Event, i.CommitInfo, i.RepoZip, i.Request.State)),
                        i =>
                        {
                            i.Request.Reporter.Send(RepositoryMessage.GetRepositoryFromDatabase);
                            Logger.LogInformation("Downloading Repository {Name} From Server", i.Request.Event.RepoName);
                            i.RepoZip.SetLength(0);
                            var dataStream = i.Request.State.Bucket.GetFile(i.Data.FileName).Open(FileAccess.Read);
                            dataStream.CopyTo(i.RepoZip);
                            i.RepoZip.Seek(0, SeekOrigin.Begin);
                        })
                   .Select(
                        r =>
                        {
                            var transferRequest = DataTransferRequest.FromStream(r.RepoZip, r.Request.Event.GetTransferManager(), TransferData.From(r.CommitInfo));
                            r.Request.State.DataTransferManager.Request(transferRequest.Inform(r.TempFiles));

                            return (r.Request, Transfer: transferRequest);
                        })
                   .Select(r => r.Request.New(OperationResult.Success(r.Transfer.OperationId)));
        }

        private bool UpdateRepository(RepositoryEntry data, Reporter reporter, TransferRepository repository, string commitInfo, Stream repozip, OperatorState state)
        {
            var (repos, bucket, _, _) = state;

            Logger.LogInformation("Try Update Repository");
            var downloadCompled = false;
            var repoConfiguration = new RepositoryConfiguration(data.SourceUrl, reporter);
            using var repoPath = RepoEnv.TempFiles.CreateDic();

            var repoName = repository.RepoName;
            var data2 = repos.AsQueryable().FirstOrDefault(r => r.RepoName == repoName);

            if (data2 is null || string.Equals(commitInfo, data2.LastUpdate, StringComparison.Ordinal)) return false;

            if (!string.IsNullOrWhiteSpace(data.FileName))
                try
                {
                    Logger.LogInformation("Downloading Repository {Name} From Server", repoName);

                    reporter.Send(RepositoryMessage.GetRepositoryFromDatabase);
                    using var file = bucket.GetFile(data.FileName).Open(FileAccess.Read);
                    file.CopyTo(repozip);

                    downloadCompled = true;
                }
                catch (Exception e)
                {
                    Logger.LogError(e, "Error on Download Repo File {Name}", data.FileName);
                }

            if (downloadCompled)
            {
                Logger.LogInformation("Unpack Repository {Name}", repoName);

                repozip.Seek(0, SeekOrigin.Begin);
                using var unpackZip = ZipFile.Read(repozip);

                reporter.Send(RepositoryMessage.ExtractRepository);
                unpackZip.ExtractAll(repoPath.FullPath, ExtractExistingFileAction.OverwriteSilently);
            }

            PullAndCompress(data, reporter, repozip, repoName, repoConfiguration, repoPath, bucket, repos);

            return true;
        }

        private void PullAndCompress(
            RepositoryEntry data, Reporter reporter, Stream repozip, in RepositoryName repoName, RepositoryConfiguration repoConfiguration, ITempDic repoPath,
            IDirectory bucket, ICanUpdate<RepositoryEntry> repos)
        {

            Logger.LogInformation("Execute Git Pull for {Name}", repoName);
            using var updater = GitUpdater.GetOrNew(repoConfiguration);
            var result = updater.RunUpdate(repoPath.FullPath);
            var dataUpdate = data with { LastUpdate = result.Sha };

            Logger.LogInformation("Compress Repository {Name}", repoName);
            reporter.Send(RepositoryMessage.CompressRepository);

            if(repozip.Length != 0)
                repozip.SetLength(0);

            using (var archive = new ZipFile())
            {
                archive.AddDirectory(repoPath.FullPath, "");
                archive.Save(repozip);
            }

            repozip.Seek(0, SeekOrigin.Begin);

            Logger.LogInformation("Upload and Update Repository {Name}", repoName);
            reporter.Send(RepositoryMessage.UploadRepositoryToDatabase);
            var id = repoName.Value.Replace('/', '_') + ".zip";
            using var newFile = bucket.GetFile(id).CreateNew();
            repozip.CopyTo(newFile);

            dataUpdate = dataUpdate with { FileName = id };

            repos.Update(dataUpdate);
            repozip.Seek(0, SeekOrigin.Begin);
        }

        private static IObservable<TData> ObservableReturn<TData>(Func<TData> fac)
            => Observable.Defer(() => Observable.Return(fac()));

        public sealed record OperatorState(IRepository<RepositoryEntry, RepositoryName> Repos, IDirectory Bucket, DataTransferManager DataTransferManager, GitHubClient GitHubClient);
    }
}
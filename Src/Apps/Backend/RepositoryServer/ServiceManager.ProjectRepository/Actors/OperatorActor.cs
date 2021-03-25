using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using Octokit;
using ServiceManager.ProjectRepository.Core;
using ServiceManager.ProjectRepository.Data;
using Tauron;
using Tauron.Application.AkkaNode.Services;
using Tauron.Application.AkkaNode.Services.CleanUp;
using Tauron.Application.AkkaNode.Services.Commands;
using Tauron.Application.AkkaNode.Services.FileTransfer;
using Tauron.Application.Master.Commands.Deployment.Repository;
using Tauron.Features;
using Tauron.Operations;
using Tauron.Temp;

namespace ServiceManager.ProjectRepository.Actors
{
    public sealed class OperatorActor : ReportingActor<OperatorActor.OperatorState>
    {
        private static readonly ReaderWriterLockSlim UpdateLock = new();
        
        public static IPreparedFeature New(IMongoCollection<RepositoryEntry> repos, GridFSBucket bucket,
            IMongoCollection<ToDeleteRevision> revisions, DataTransferManager dataTransfer)
            => Feature.Create(() => new OperatorActor(), c => new OperatorState(repos, bucket, revisions, dataTransfer,
                new GitHubClient(new ProductHeaderValue(
                    c.System.Settings.Config.GetString("akka.appinfo.applicationName",
                        "Test Apps").Replace(' ', '_'))),
                new Dictionary<string, ITempFile>()));

        protected override void ConfigImpl()
        {
            Receive<RegisterRepository>("RegisterRepository", RegisterRepository);

            Receive<TransferRepository>("RequestRepository", RequestRepository);

            Receive<TransferMessages.TransferCompled>(obs =>
                obs.SubscribeWithStatus(c =>
                {
                    if (c.State.CurrentTransfers.TryGetValue(c.Event.OperationId, out var f))
                        f.Dispose();
                    Context.Stop(Self);
                }));
        }

        private void RegisterRepository(StatePair<RegisterRepository, OperatorState> msg, Reporter reporter)
        {
            var ((repoName, ignoreDuplicate), (repos, _, _, _, gitHubClient, _), _) = msg;

            UpdateLock.EnterUpgradeableReadLock();
            try
            {
                Log.Info("Incomming Registration Request for Repository {Name}", repoName);

                reporter.Send(RepositoryMessages.GetRepo);
                var data = repos.AsQueryable().FirstOrDefault(e => e.RepoName == repoName);

                if (data != null)
                {
                    Log.Info("Repository {Name} is Registrated", repoName);
                    if (ignoreDuplicate)
                    {
                        reporter.Compled(OperationResult.Success());
                        return;
                    }

                    reporter.Compled(OperationResult.Failure(RepoErrorCodes.DuplicateRepository));
                    return;
                }

                if (!repoName.Contains('/'))
                {
                    Log.Info("Repository {Name} Name is Invalid", repoName);
                    reporter.Compled(OperationResult.Failure(RepoErrorCodes.InvalidRepoName));
                    return;
                }

                var nameSplit = repoName.Split('/');
                var repoInfo = gitHubClient.Repository.Get(nameSplit[0], nameSplit[1]).Result;

                if (repoInfo == null)
                {
                    Log.Info("Repository {Name} Name not found on Github", repoName);
                    reporter.Compled(OperationResult.Failure(RepoErrorCodes.GithubNoRepoFound));
                    return;
                }

                Log.Info("Savin new Repository {Name} on Database", repoName);
                data = new RepositoryEntry
                {
                    RepoName = repoName,
                    SourceUrl = repoInfo.CloneUrl,
                    RepoId = repoInfo.Id
                };

                UpdateLock.EnterWriteLock();
                try
                {
                    repos.InsertOne(data);
                }
                finally
                {
                    UpdateLock.ExitWriteLock();
                }

                reporter.Compled(OperationResult.Success());
            }
            finally
            {
                UpdateLock.ExitUpgradeableReadLock();
                Context.Stop(Self);
            }
        }

        private void RequestRepository(StatePair<TransferRepository, OperatorState> msg, Reporter reporter)
        {
            var (repository, (repos, bucket, _, dataTransfer, gitHubClient, currentTransfers), _) = msg;

            var repozipFile = RepoEnv.TempFiles.CreateFile();
            UpdateLock.EnterUpgradeableReadLock();
            try
            {
                Log.Info("Incomming Transfer Request for Repository {Name}", repository.RepoName);
                reporter.Send(RepositoryMessages.GetRepo);

                var data = repos.AsQueryable().FirstOrDefault(r => r.RepoName == repository.RepoName);
                if (data == null)
                {
                    reporter.Compled(OperationResult.Failure(RepoErrorCodes.DatabaseNoRepoFound));
                    return;
                }

                var commitInfo = gitHubClient.Repository.Commit.GetSha1(data.RepoId, "HEAD").Result;

                var repozip = repozipFile.Stream;

                if (!(commitInfo != data.LastUpdate &&
                      UpdateRepository(data, reporter, repository, commitInfo, repozip, msg.State)))
                {
                    reporter.Send(RepositoryMessages.GetRepositoryFromDatabase);
                    Log.Info("Downloading Repository {Name} From Server", repository.RepoName);
                    repozip.SetLength(0);
                    bucket.DownloadToStream(ObjectId.Parse(data.FileName), repozip);
                }

                //_reporter = reporter;

                //repozip.Seek(0, SeekOrigin.Begin);
                //Timers.StartSingleTimer(_reporter, new TransferFailed(string.Empty, FailReason.Timeout, data.RepoName), TimeSpan.FromMinutes(10));
                // ReSharper disable once NotResolvedInText
                var request = DataTransferRequest.FromStream(repository.OperationId, repozip,
                    repository.Manager ?? throw new ArgumentNullException(@"FileManager"), commitInfo);
                request.SendCompletionBack = true;

                dataTransfer.Request(request);
                currentTransfers[request.OperationId] = repozipFile;

                reporter.Compled(OperationResult.Success(new FileTransactionId(request.OperationId)));
            }
            finally
            {
                UpdateLock.ExitUpgradeableReadLock();
            }
        }

        private bool UpdateRepository(RepositoryEntry data, Reporter reporter, TransferRepository repository,
            string commitInfo, Stream repozip, OperatorState state)
        {
            var (repos, bucket, revisions, _, _, _) = state;

            Log.Info("Try Update Repository");
            UpdateLock.EnterWriteLock();
            try
            {
                var downloadCompled = false;
                var repoConfiguration = new RepositoryConfiguration(data.SourceUrl, reporter);
                using var repoPath = RepoEnv.TempFiles.CreateDic();

                var (repoName, _) = repository;
                var data2 = repos.AsQueryable().FirstOrDefault(r => r.RepoName == repoName);
                if (data2 != null && commitInfo != data2.LastUpdate)
                {
                    if (!string.IsNullOrWhiteSpace(data.FileName))
                        try
                        {
                            Log.Info("Downloading Repository {Name} From Server", repoName);

                            reporter.Send(RepositoryMessages.GetRepositoryFromDatabase);
                            bucket.DownloadToStream(ObjectId.Parse(data.FileName), repozip);

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
                        using var unpackZip = new ZipArchive(repozip);

                        reporter.Send(RepositoryMessages.ExtractRepository);
                        unpackZip.ExtractToDirectory(repoPath.FullPath);
                    }

                    Log.Info("Execute Git Pull for {Name}", repoName);
                    using var updater = GitUpdater.GetOrNew(repoConfiguration);
                    var result = updater.RunUpdate(repoPath.FullPath);
                    var dataUpdate = Builders<RepositoryEntry>.Update.Set(e => e.LastUpdate, result.Sha);

                    Log.Info("Compress Repository {Name}", repoName);
                    reporter.Send(RepositoryMessages.CompressRepository);

                    if (repozip.Length != 0)
                        repozip.SetLength(0);

                    using (var archive = new ZipArchive(repozip, ZipArchiveMode.Create, true))
                    {
                        archive.AddFilesFromDictionary(repoPath.FullPath);
                    }

                    repozip.Seek(0, SeekOrigin.Begin);

                    Log.Info("Upload and Update Repository {Name}", repoName);
                    reporter.Send(RepositoryMessages.UploadRepositoryToDatabase);
                    var current = data.FileName;
                    var id = bucket.UploadFromStream(repoName.Replace('/', '_') + ".zip", repozip);
                    dataUpdate = dataUpdate.Set(e => e.FileName, id.ToString());

                    if (!string.IsNullOrWhiteSpace(current))
                        revisions.InsertOne(new ToDeleteRevision(current));

                    repos.UpdateOne(e => e.RepoName == data.RepoName, dataUpdate);
                    data.FileName = id.ToString();

                    repozip.Seek(0, SeekOrigin.Begin);

                    return true;
                }
            }
            finally
            {
                UpdateLock.ExitWriteLock();
            }

            return false;
        }

        public sealed record OperatorState(IMongoCollection<RepositoryEntry> Repos, GridFSBucket Bucket,
            IMongoCollection<ToDeleteRevision> Revisions, DataTransferManager DataTransferManager,
            GitHubClient GitHubClient, Dictionary<string, ITempFile> CurrentTransfers);
    }
}
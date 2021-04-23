using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using LibGit2Sharp;
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

namespace ServiceManager.ProjectRepository.Actors
{
    public class temp
    {
        private void RequestRepository(StatePair<TransferRepository, OperatorActor.OperatorState> msg, Reporter reporter)
        {
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
    }
}
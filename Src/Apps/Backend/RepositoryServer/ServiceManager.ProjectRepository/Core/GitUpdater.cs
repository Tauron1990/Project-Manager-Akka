using System;
using System.IO;
using LibGit2Sharp;
using Tauron.Application.Master.Commands.Deployment.Repository;

namespace ServiceManager.ProjectRepository.Core
{
    #pragma warning disable MA0064
    
    public sealed class GitUpdater : SharedObject<GitUpdater, RepositoryConfiguration>
    {
        private Repository? _repository;

        private void Init(string sourceDic)
        {
            if (_repository != null)
                return;

            if (!Directory.Exists(sourceDic)) return;

            var path = Repository.Discover(sourceDic);

            if (!string.IsNullOrWhiteSpace(path))
                _repository = new Repository(path);
        }

        public (string Path, string Sha) RunUpdate(string source)
        {
            #pragma warning disable MT1000
            lock (Lock)
            {
                Init(source);

                if (_repository is null)
                {
                    SendMessage(RepositoryMessage.DownloadRepository);

                    return Download(source);
                }

                SendMessage(RepositoryMessage.UpdateRepository);

                return Update();
            }
        }

        private (string Path, string Sha) Update()
        {
            if (_repository is null)
                throw new InvalidOperationException("Not Repository Set");

            Commands.Pull(
                _repository,
                new Signature("ServiceManager", "Service@Manager.com", DateTimeOffset.Now),
                new PullOptions());

            return (_repository.Info.WorkingDirectory, _repository.Head.Tip.Sha);
        }

        private (string Path, string Sha) Download(string source)
        {
            if(!Directory.Exists(source))
                Directory.CreateDirectory(source);
            
            _repository = new Repository(Repository.Clone(Configuration.CloneUrl, source));

            return (_repository.Info.WorkingDirectory, _repository.Head.Tip.Sha);
        }

        protected override void InternalDispose()
        {
            lock (Lock)
            {
                _repository?.Dispose();
            }

            base.InternalDispose();
        }
    }
}
#pragma warning restore MT1000
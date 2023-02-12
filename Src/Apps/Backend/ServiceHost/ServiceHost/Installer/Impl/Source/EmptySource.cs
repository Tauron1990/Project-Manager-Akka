using System;
using System.Threading.Tasks;
using Akka.Actor;
using Tauron.Application.Master.Commands;

namespace ServiceHost.Installer.Impl.Source
{
    public sealed class EmptySource : IInstallationSource
    {
        private EmptySource() { }

        public static EmptySource Instnace { get; } = new();

        public Status ValidateInput(InstallerContext name) => new Status.Failure(new NotImplementedException());

        public Task<Status> PrepareforCopy(InstallerContext context)
            => Task.FromResult<Status>(new Status.Failure(new NotImplementedException()));

        public Task<Status> CopyTo(InstallerContext context, string target)
            => Task.FromResult<Status>(new Status.Failure(new NotImplementedException()));

        public void CleanUp(InstallerContext context) { }

        public SimpleVersion Version => SimpleVersion.NoVersion;

        public string ToZipFile(InstallerContext context)
            => throw new NotSupportedException("Empty Source can not converted to zip file");
    }
}
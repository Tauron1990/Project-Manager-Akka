using System.Threading.Tasks;
using Akka.Actor;
using Tauron.Application.Master.Commands;

namespace ServiceHost.Installer.Impl.Source
{
    public interface IInstallationSource
    {
        SimpleVersion Version { get; }
        Status ValidateInput(InstallerContext context);

        Task<Status> PrepareforCopy(InstallerContext context);

        Task<Status> CopyTo(InstallerContext context, string target);

        void CleanUp(InstallerContext context);

        string ToZipFile(InstallerContext context);
    }
}
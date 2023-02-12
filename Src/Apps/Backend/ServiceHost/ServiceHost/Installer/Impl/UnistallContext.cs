using ServiceHost.ApplicationRegistry;
using Tauron.Application.ActorWorkflow;
using Tauron.Application.Master.Commands;

namespace ServiceHost.Installer.Impl
{
    public sealed class UnistallContext : IWorkflowContext
    {
        public UnistallContext(AppName name) => Name = name;
        public Backup Backup { get; } = new();

        public Recovery Recovery { get; } = new();

        public AppName Name { get; }

        public InstalledApp App { get; set; } = InstalledApp.Empty;
    }
}
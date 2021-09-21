using ServiceHost.ApplicationRegistry;
using Tauron.Application.ActorWorkflow;

namespace ServiceHost.Installer.Impl
{
    public sealed class UnistallContext : IWorkflowContext
    {
        public UnistallContext(string name) => Name = name;
        public Backup Backup { get; } = new();

        public Recovery Recovery { get; } = new();

        public string Name { get; }

        public InstalledApp App { get; set; } = InstalledApp.Empty;
    }
}
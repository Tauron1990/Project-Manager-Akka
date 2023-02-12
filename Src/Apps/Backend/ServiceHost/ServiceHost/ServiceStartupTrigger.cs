using Akka.Actor;
using Akka.Cluster;
using ServiceHost.Installer;
using ServiceHost.Services;
using Tauron.Application.Master.Commands.Administration.Host;

namespace ServiceHost
{
    public sealed class ServiceStartupTrigger
    {
        private readonly InstallChecker _checker;
        private readonly IAppManager _manager;
        private readonly ActorSystem _system;

        public ServiceStartupTrigger(IAppManager manager, ActorSystem system, InstallChecker checker)
        {
            _manager = manager;
            _system = system;
            _checker = checker;
        }

        public void Run()
        {
            if (_checker.IsInstallationStart)
                return;

            _manager.Tell(new StartApps(AppType.StartUp));
            Cluster.Get(_system).RegisterOnMemberUp(() => _manager.Tell(new StartApps(AppType.Cluster)));
        }
    }
}
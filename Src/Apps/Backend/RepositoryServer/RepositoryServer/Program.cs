using System.Threading.Tasks;
using ServiceManager.ProjectRepository;
using Tauron.Application.AkkaNode.Bootstrap;
using Tauron.Application.AkkaNode.Bootstrap.Console;
using Tauron.Application.AkkaNode.Services.FileTransfer;
using Tauron.Application.Master.Commands;

namespace RepositoryServer
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            await Bootstrap.StartNode(args, KillRecpientType.Service, IpcApplicationType.Client)
               .ConfigurateAkkaSystem((_, system) =>
                    {
                        RepositoryManager.InitRepositoryManager(
                            system, 
                            system.Settings.Config.GetString("akka.persistence.journal.mongodb.connection-string"),
                            DataTransferManager.New(system, "Data-Tranfer-Manager"));
                    })
               .Build().Run();
        }
    }
}

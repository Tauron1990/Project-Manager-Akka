using JetBrains.Annotations;
using Tauron.Application.AkkNode.Services;

namespace ServiceManager.ProjectRepository.Core
{
    [PublicAPI]
    public record RepositoryConfiguration(string CloneUrl, Reporter? Logger) : IReporterProvider
    {
        //public const string RepositoryLockName = "{01324768-0950-49F9-AB39-CA94E4252C55}-ServiceManager-ProjectManagerRepository";

        //private const string DefaultDotNetPath = @"C:\Program Files\dotnet\dotnet.exe";
        //private const string DefaultSoloution = "Project-Manager-Akka.sln";
        private const string DefaultUrl = "https://github.com/Tauron1990/Project-Manager-Akka.git";

        //public string DotNetPath { get; set; } = DefaultDotNetPath;

        //public string Solotion { get; set; } = DefaultSoloution;

        
        public RepositoryConfiguration()
            : this(DefaultUrl, null)
        {
            
        }

        void IReporterProvider.SendMessage(string msg) => Logger?.Send(msg);
    }
}

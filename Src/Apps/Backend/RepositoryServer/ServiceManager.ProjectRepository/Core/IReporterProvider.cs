using Tauron.Application.Master.Commands.Deployment.Repository;

namespace ServiceManager.ProjectRepository.Core
{
    public interface IReporterProvider
    {
        void SendMessage(RepositoryMessage msg);
    }
}
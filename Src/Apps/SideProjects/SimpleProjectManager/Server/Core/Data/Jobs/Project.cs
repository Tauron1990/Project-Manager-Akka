using Akkatecture.Aggregates;
using SimpleProjectManager.Shared;

namespace SimpleProjectManager.Server.Core.Data
{
    public sealed class Project : AggregateRoot<Project, ProjectId, ProjectState>,
        IExecute<CreateProjectCommandCarrier>
    {
        public Project(ProjectId id) 
            : base(id) { }

        public bool Execute(CreateProjectCommandCarrier command)
        {
            try
            {

            }
            catch (Exception e)
            {
                Console.WriteLine(e);

                throw;
            }
        }
    }
}

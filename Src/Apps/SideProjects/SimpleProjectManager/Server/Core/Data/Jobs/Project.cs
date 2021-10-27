using Akkatecture.Aggregates;
using SimpleProjectManager.Shared;
using Tauron.Operations;

namespace SimpleProjectManager.Server.Core.Data
{
    public sealed class Project : InternalAggregateRoot<Project, ProjectId, ProjectState>,
        IExecute<CreateProjectCommandCarrier>
    {
        public Project(ProjectId id) 
            : base(id) { }

        public bool Execute(CreateProjectCommandCarrier command)
            => Run(
                command,
                com =>
                {
                    if(!IsNew) return OperationResult.Failure(new Error($"Der Job {command.Command.Project} ist nicht neu", Errors.ProjectNoNewError));

                    return OperationResult.Success();
                });
    }
}

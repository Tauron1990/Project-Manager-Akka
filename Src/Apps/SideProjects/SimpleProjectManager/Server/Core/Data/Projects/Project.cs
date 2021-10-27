using Akkatecture.Aggregates;
using SimpleProjectManager.Server.Core.Data.Events;
using SimpleProjectManager.Shared;
using SimpleProjectManager.Shared.Validators;
using Tauron.Operations;

namespace SimpleProjectManager.Server.Core.Data
{
    public sealed class Project : InternalAggregateRoot<Project, ProjectId, ProjectState, ProjectStateSnapshot>,
        IExecute<CreateProjectCommandCarrier>
    {
        private readonly CreateProjectCommandValidator _createProjectCommandValidator = new();

        public Project(ProjectId id) 
            : base(id) { }

        public bool Execute(CreateProjectCommandCarrier command)
        {
            return Run(
                command,
                com =>
                {
                    if (!IsNew) return OperationResult.Failure(new Error($"Der Job {command.Data.Project} ist nicht neu", Errors.ProjectNoNewError));

                    var validationResult = _createProjectCommandValidator.Validate(com.Data);

                    if (!validationResult.IsValid)
                        return OperationResult.Failure(validationResult.Errors.Select(err => new Error(err.ErrorMessage, err.ErrorCode)));

                    var data = com.Data;

                    EmitAll(
                        new NewProjectCreatedEvent(data.Project),
                        new ProjectFilesAttachedEvent(data.Files),
                        new ProjectDeadLineChangedEvent(data.Deadline),
                        new ProjectStatusChangedEvent(data.Status)
                    );

                    return OperationResult.Success();
                });
        }
    }
}

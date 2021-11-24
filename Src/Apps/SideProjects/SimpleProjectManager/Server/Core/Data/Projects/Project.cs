using Akkatecture.Aggregates;
using FluentValidation;
using SimpleProjectManager.Server.Core.Data.Events;
using SimpleProjectManager.Shared;
using SimpleProjectManager.Shared.Validators;
using Tauron.Operations;

namespace SimpleProjectManager.Server.Core.Data
{
    public sealed class Project : InternalAggregateRoot<Project, ProjectId, ProjectState, ProjectStateSnapshot>,
        IExecute<CreateProjectCommandCarrier>, IExecute<UpdateProjectCommandCarrier>, IExecute<>
    {
        private readonly IValidator<CreateProjectCommandCarrier> _createProjectCommandValidator 
            = CreateValidator<CreateProjectCommandCarrier, CreateProjectCommand>(new CreateProjectCommandValidator());

        private readonly IValidator<UpdateProjectCommandCarrier> _updateCommandValidator =
            CreateValidator<UpdateProjectCommandCarrier, UpdateProjectCommand>(new UpdateProjectCommandValidator());

        public Project(ProjectId id) 
            : base(id) { }

        protected override string GetErrorMessage(string errorCode)
            => errorCode switch
            {
                Errors.NoNewError => "Der Job ist nicht neu",
                Errors.NewError => "Der Job wurde nicht gefunden",
                _ => errorCode
            };

        public bool Execute(CreateProjectCommandCarrier command)
            => Run(command,
                _createProjectCommandValidator,
                AggregateNeed.New,
                com =>
                {
                    var data = com.Command;
                    
                    EmitAll(
                        new NewProjectCreatedEvent(data.Project),
                        new ProjectFilesAttachedEvent(data.Files),
                        new ProjectDeadLineChangedEvent(data.Deadline),
                        new ProjectStatusChangedEvent(data.Status)
                    );

                    return OperationResult.Success();
                });

        public bool Execute(UpdateProjectCommandCarrier command)
            => Run(command,
                _updateCommandValidator,
                AggregateNeed.Exist,
                com =>
                {
                    var data = com.Command;

                    IEnumerable<IAggregateEvent<Project, ProjectId>> CreateEvents()
                    {
                        if (data.Name is not null)
                            yield return new ProjectNameChangedEvent(data.Name);

                        if (data.Status is not null)
                            yield return new ProjectStatusChangedEvent(data.Status.Value);

                        if (data.Deadline is not null)
                            yield return new ProjectDeadLineChangedEvent(data.Deadline.Data);
                    }

                    EmitAll(CreateEvents());

                    return OperationResult.Success();
                });
    }
}

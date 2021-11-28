using Akkatecture.Aggregates;
using FluentValidation;
using SimpleProjectManager.Server.Core.Data.Events;
using SimpleProjectManager.Shared;
using SimpleProjectManager.Shared.Validators;
using Tauron.Operations;

namespace SimpleProjectManager.Server.Core.Data
{
    public sealed class Project : InternalAggregateRoot<Project, ProjectId, ProjectState, ProjectStateSnapshot>,
        IExecute<CreateProjectCommandCarrier>, IExecute<UpdateProjectCommandCarrier>, IExecute<ProjectRemoveFilesCommandCarrier>,
        IExecute<ProjectAttachFilesCommandCarrier>, IExecute<ProjectDeleteCommandCarrier>
    {
        private readonly IValidator<CreateProjectCommandCarrier> _createProjectCommandValidator 
            = CreateValidator<CreateProjectCommandCarrier, CreateProjectCommand>(new CreateProjectCommandValidator());

        private readonly IValidator<UpdateProjectCommandCarrier> _updateCommandValidator 
            = CreateValidator<UpdateProjectCommandCarrier, UpdateProjectCommand>(new UpdateProjectCommandValidator());

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
        {
            return Run(
                command,
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
        }

        public bool Execute(UpdateProjectCommandCarrier command)
        {
            return Run(
                command,
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

        public bool Execute(ProjectRemoveFilesCommandCarrier command)
        {
            return Run(
                command,
                null,
                AggregateNeed.Exist,
                com =>
                {
                    Emit(new ProjectFilesRemovedEvent(com.Command.Files));

                    return OperationResult.Success();
                });
        }

        public bool Execute(ProjectAttachFilesCommandCarrier command)
        {
            try
            {
                var evt = new ProjectFilesAttachedEvent(command.Command.Files);
                if (IsNew)
                {
                    var validator = new ProjectNameValidator();
                    var validationResult = validator.Validate(command.Command.Name);
                    if (validationResult.IsValid)
                    {
                        EmitAll(new NewProjectCreatedEvent(command.Command.Name), evt);
                        TellSenderIsPresent(OperationResult.Success(true));
                    }
                    else
                        TellSenderIsPresent(CreateFailure(validationResult));
                }
                else
                {
                    Emit(evt);
                    TellSenderIsPresent(OperationResult.Success(false));
                }
            
                return true;
            }
            catch (Exception e)
            {
                TellSenderIsPresent(OperationResult.Failure(e));
            }

            return true;
        }

        public bool Execute(ProjectDeleteCommandCarrier command)
        {
            return Run(
                command,
                null,
                AggregateNeed.Exist,
                _ =>
                {
                    Emit(new ProjectDeletedEvent());
                    
                    return OperationResult.Success();
                });
        }
    }
}

using Akkatecture.Aggregates;
using FluentValidation;
using SimpleProjectManager.Server.Core.Data.Events;
using SimpleProjectManager.Shared;
using SimpleProjectManager.Shared.Validators;
using Tauron.Operations;

namespace SimpleProjectManager.Server.Core.Data
{
    public sealed class Project : InternalAggregateRoot<Project, ProjectId, ProjectState, ProjectStateSnapshot>,
        IExecute<CreateProjectCommandCarrier>
    {
        private readonly IValidator<CreateProjectCommandCarrier> _createProjectCommandValidator =
            new InlineValidator<CreateProjectCommandCarrier>
            {
                v => v.RuleFor(c => c.Data).SetValidator(new CreateProjectCommandValidator())
            };

        public Project(ProjectId id) 
            : base(id) { }

        public bool Execute(CreateProjectCommandCarrier command)
        {
            return Run(
                command,
                _createProjectCommandValidator,
                AggregateNeed.New,
                com =>
                {
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

        protected override string GetErrorMessage(string errorCode)
            => errorCode switch
            {
                Errors.NewError => "Der Job ist nicht neu",
                Errors.NoNewError => "Der Job wurde nicht gefunde",
                _ => errorCode
            };
    }
}

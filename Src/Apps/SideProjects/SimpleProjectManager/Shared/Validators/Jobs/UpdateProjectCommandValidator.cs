using FluentValidation;

namespace SimpleProjectManager.Shared.Validators;

public class UpdateProjectCommandValidator : AbstractValidator<UpdateProjectCommand>
{
    public UpdateProjectCommandValidator()
    {
        RuleFor(c => c.Name).SetValidator(new ProjectNameValidator()!).When(c => c.Name is not null);
        RuleFor(c => c.Deadline).SetValidator(GenericNullDataValidator.Create(new ProjectDeadlineValidator())!).When(c => c.Deadline is not null);
    }
}
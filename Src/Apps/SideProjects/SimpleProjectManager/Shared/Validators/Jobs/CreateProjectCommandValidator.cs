using FluentValidation;

namespace SimpleProjectManager.Shared.Validators;

public sealed class CreateProjectCommandValidator : AbstractValidator<CreateProjectCommand>
{
    public CreateProjectCommandValidator()
    {
        RuleFor(c => c.Files).NotNull();
        RuleFor(c => c.Project).NotNull().SetValidator(new ProjectNameValidator());
        RuleFor(c => c.Deadline).SetValidator(new ProjectDeadlineValidator()!).When(c => c.Deadline is not null);
        RuleFor(c => c.Status).IsInEnum();
    }
}
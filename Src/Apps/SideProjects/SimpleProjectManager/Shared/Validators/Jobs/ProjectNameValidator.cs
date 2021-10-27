using FluentValidation;

namespace SimpleProjectManager.Shared.Validators;

public class ProjectNameValidator : AbstractValidator<ProjectName>
{
    public ProjectNameValidator()
    {
        RuleFor(jn => jn.Value).NotEmpty();
    }
}
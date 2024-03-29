﻿using FluentValidation;

namespace SimpleProjectManager.Shared.Validators;

public sealed class ProjectDeadlineValidator : AbstractValidator<ProjectDeadline>
{
    public ProjectDeadlineValidator()
    {
        RuleFor(pd => pd.Value).Must(d => d > DateTimeOffset.UtcNow).WithMessage("Der Termin muss in der Zukunft Liegen");
    }
}
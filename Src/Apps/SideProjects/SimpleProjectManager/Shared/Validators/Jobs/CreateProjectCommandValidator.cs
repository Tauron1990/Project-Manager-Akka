﻿using Akkatecture.Jobs;
using FluentValidation;

namespace SimpleProjectManager.Shared.Validators;

public sealed class CreateProjectCommandValidator : AbstractValidator<CreateProjectCommand>
{
    public CreateProjectCommandValidator()
    {
        RuleFor(c => c.Files).NotNull();
        RuleFor(c => c.Project).SetValidator(new ProjectNameValidator());
        RuleFor(c => c.Deadline).SetValidator(new ProjectDeadlineValidator()!).When(c => c.Deadline is not null);
    }
}
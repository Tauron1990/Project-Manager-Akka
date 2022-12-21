using FluentAssertions;
using FluentValidation.Results;
using SimpleProjectManager.Shared.Tests.TestData;
using SimpleProjectManager.Shared.Validators;

namespace SimpleProjectManager.Shared.Tests.Validators.Jobs;

public sealed class CreateProjectCommandValidatorTests
{
    [Theory, DomainAutoData]
    public void Valid_Command(CreateProjectCommand command)
    {
        ValidationResult result = new CreateProjectCommandValidator().Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Theory, DomainAutoData]
    public void Null_Name_Command(CreateProjectCommand command)
    {
        command = command with { Project = null! };

        ValidationResult result = new CreateProjectCommandValidator().Validate(command);

        result.IsValid.Should().BeFalse();
    }

    [Theory, DomainAutoData]
    public void No_Deadline_Command(CreateProjectCommand command)
    {
        command = command with { Deadline = null };

        ValidationResult result = new CreateProjectCommandValidator().Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Theory, DomainAutoData]
    public void Null_Files_Command(CreateProjectCommand command)
    {
        command = command with { Files = null! };

        ValidationResult result = new CreateProjectCommandValidator().Validate(command);

        result.IsValid.Should().BeFalse();
    }
    
    
}
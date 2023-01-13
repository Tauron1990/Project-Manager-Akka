using FluentAssertions;
using FluentValidation.Results;
using SimpleProjectManager.Shared.Tests.TestData;
using SimpleProjectManager.Shared.Validators;

namespace SimpleProjectManager.Shared.Tests.Validators.Jobs;

public sealed class UpdateProjectCommandValidatorTests
{
    [Theory]
    [DomainAutoData]
    public void Valid_Command(UpdateProjectCommand command)
    {
        ValidationResult result = new UpdateProjectCommandValidator().Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [DomainAutoData]
    public void Null_Id_Command(UpdateProjectCommand command)
    {
        command = command with { Id = null! };

        ValidationResult result = new UpdateProjectCommandValidator().Validate(command);

        result.IsValid.Should().BeFalse();
    }
}
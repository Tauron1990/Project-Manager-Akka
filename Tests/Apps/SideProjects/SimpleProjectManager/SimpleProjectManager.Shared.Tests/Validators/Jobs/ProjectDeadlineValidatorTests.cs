using FluentAssertions;
using FluentValidation.Results;
using SimpleProjectManager.Shared.Tests.TestData;
using SimpleProjectManager.Shared.Validators;

namespace SimpleProjectManager.Shared.Tests.Validators.Jobs;

public sealed class ProjectDeadlineValidatorTests
{
    [Theory, DomainAutoData]
    public void Valid_Deadline(ProjectDeadline deadline)
    {
        ValidationResult result = new ProjectDeadlineValidator().Validate(deadline);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void InValid_Deadline()
    {
        var deadline = new ProjectDeadline(DateTime.Now - TimeSpan.FromDays(10));

        ValidationResult result = new ProjectDeadlineValidator().Validate(deadline);

        result.IsValid.Should().BeFalse();
    }
}
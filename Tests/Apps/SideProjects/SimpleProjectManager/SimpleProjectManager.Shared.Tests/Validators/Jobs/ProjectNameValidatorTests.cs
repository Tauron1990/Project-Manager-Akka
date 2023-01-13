using FluentAssertions;
using FluentValidation.Results;
using SimpleProjectManager.Shared.Tests.TestData;
using SimpleProjectManager.Shared.Validators;

namespace SimpleProjectManager.Shared.Tests.Validators.Jobs;

public sealed class ProjectNameValidatorTests
{
    [Theory]
    [DomainAutoData]
    public void Valid_Name(ProjectName name)
    {
        ValidationResult result = new ProjectNameValidator().Validate(name);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Invalid_Name()
    {
        var name = new ProjectName("");

        ValidationResult result = new ProjectNameValidator().Validate(name);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void BM_Case_Validation()
    {
        var name = new ProjectName("BM22_10000");

        ValidationResult result = new ProjectNameValidator().Validate(name);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void BM_Case_Invalid_Year()
    {
        var nameLong = new ProjectName("BM999_10000");
        var nameShort = new ProjectName("BM9_10000");

        ValidationResult resultLong = new ProjectNameValidator().Validate(nameLong);
        ValidationResult resultShort = new ProjectNameValidator().Validate(nameShort);

        resultLong.IsValid.Should().BeFalse();
        resultShort.IsValid.Should().BeFalse();
    }

    [Fact]
    public void BM_Case_Invalid_Project_Count()
    {
        var nameLong = new ProjectName("BM22_100000");
        var nameShort = new ProjectName("BM22_1000");

        ValidationResult resultLong = new ProjectNameValidator().Validate(nameLong);
        ValidationResult resultShort = new ProjectNameValidator().Validate(nameShort);

        resultLong.IsValid.Should().BeFalse();
        resultShort.IsValid.Should().BeFalse();
    }
}
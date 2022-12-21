using FluentAssertions;
using FluentValidation.Results;
using SimpleProjectManager.Shared.Tests.TestData;
using SimpleProjectManager.Shared.Validators;

namespace SimpleProjectManager.Shared.Tests.Validators.Jobs;

public sealed class ProjectNameValidatorTests
{
    [Theory, DomainAutoData]
    public void Valid_Name(ProjectName name)
    {
        ValidationResult result = new ProjectNameValidator().Validate(name);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Invalid_Name()
    {
        ProjectName name = new ProjectName("");

        ValidationResult result = new ProjectNameValidator().Validate(name);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void BM_Case_Validation()
    {
        ProjectName name = new ProjectName("BM22_10000");

        ValidationResult result = new ProjectNameValidator().Validate(name);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void BM_Case_Invalid_Year()
    {
        ProjectName nameLong = new ProjectName("BM999_10000");
        ProjectName nameShort = new ProjectName("BM9_10000");
        
        ValidationResult resultLong = new ProjectNameValidator().Validate(nameLong);
        ValidationResult resultShort = new ProjectNameValidator().Validate(nameShort);
        
        resultLong.IsValid.Should().BeFalse();
        resultShort.IsValid.Should().BeFalse();
    }
    
    [Fact]
    public void BM_Case_Invalid_Project_Count()
    {
        ProjectName nameLong = new ProjectName("BM22_100000");
        ProjectName nameShort = new ProjectName("BM22_1000");
        
        ValidationResult resultLong = new ProjectNameValidator().Validate(nameLong);
        ValidationResult resultShort = new ProjectNameValidator().Validate(nameShort);
        
        resultLong.IsValid.Should().BeFalse();
        resultShort.IsValid.Should().BeFalse();
    }
}
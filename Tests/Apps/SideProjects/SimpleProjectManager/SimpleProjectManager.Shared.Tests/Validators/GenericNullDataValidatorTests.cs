using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using SimpleProjectManager.Shared.Tests.TestData;
using SimpleProjectManager.Shared.Validators;

namespace SimpleProjectManager.Shared.Tests.Validators;

public sealed class GenericNullDataValidatorTests
{
    [Theory, DomainAutoData]
    public void Non_Null_Valid_Validator(NullableData<string> value, IValidator<string> validator)
    {
        ValidationResult result = GenericNullDataValidator.Create(validator).Validate(value);

        result.Should().NotBeNull();
        result.IsValid.Should().BeTrue();
    }

    [Theory, DomainAutoData]
    public void No_Null_InValid_Validator(IValidator<string> validator)
    {
        ValidationResult result = GenericNullDataValidator.Create(validator).Validate(new NullableData<string>(string.Empty));

        result.Should().NotBeNull();
        result.IsValid.Should().BeFalse();
    }


}
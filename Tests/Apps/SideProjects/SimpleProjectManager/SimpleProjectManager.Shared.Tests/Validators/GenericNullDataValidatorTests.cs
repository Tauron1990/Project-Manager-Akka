using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using SimpleProjectManager.Shared.Tests.TestData;
using SimpleProjectManager.Shared.Validators;

namespace SimpleProjectManager.Shared.Tests.Validators;

public sealed class GenericNullDataValidatorTests
{
    [Theory, DomainAutoData]
    public void NonNullValidator(NullableData<string> value, IValidator<string> validator)
    {
        ValidationResult result = GenericNullDataValidator.Create(validator).Validate(value);

        result.Should().NotBeNull();
        result.IsValid.Should().BeTrue();
    }

    [Theory, DomainAutoData]
    public void NoNullInValidValidator(IValidator<string> validator)
    {
        
    }
}
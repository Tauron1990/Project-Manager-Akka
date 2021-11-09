using FluentValidation;

namespace SimpleProjectManager.Shared.Validators;

public static class GenericNullDataValidator
{
    public static IValidator<NullableData<TData>?> Create<TData>(IValidator<TData> validator)
        => new GenericNullDataValidator<TData>(validator);
}

public sealed class GenericNullDataValidator<TData> : AbstractValidator<NullableData<TData>?>
{
    internal GenericNullDataValidator(IValidator<TData> validator)
        => RuleFor(d => d!.Data).SetValidator(validator).When(d => d is not null);
}
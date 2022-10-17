using Vogen;

namespace Tauron.Application.Master.Commands.Deployment;

[ValueObject(typeof(string))]
[Instance("Empty", "")]
public readonly partial struct RepositoryName
{
    private static Validation Validate(string value)
        => string.IsNullOrWhiteSpace(value) ? Validation.Invalid("Repository name should not bet Empty") : Validation.Ok;
}
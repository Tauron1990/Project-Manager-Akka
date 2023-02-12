using Vogen;

namespace Tauron.Application.Master.Commands.Deployment;

[ValueObject(typeof(string))]
[Instance("Empty", "")]
#pragma warning disable MA0097
public readonly partial struct RepositoryName
    #pragma warning restore MA0097
{
    public bool IsValid => Value.Contains('/', System.StringComparison.Ordinal);

    public string[] GetSegments() => Value.Split('/');
    
    private static Validation Validate(string value)
        => string.IsNullOrWhiteSpace(value) ? Validation.Invalid("Repository name should not bet Empty") : Validation.Ok;
}
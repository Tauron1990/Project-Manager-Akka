using Vogen;

namespace Tauron.Application.Master.Commands.Deployment.Build;

[ValueObject(typeof(string))]
[Instance("Empty", "")]
public readonly partial struct ProjectName
{
    private static Validation Validate(string value)
        => string.IsNullOrWhiteSpace(value) ? Validation.Invalid("Project Name shiould not be Empty") : Validation.Ok;
}
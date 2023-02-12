using Vogen;
using Tauron.Operations;

namespace Tauron.Application.Master.Commands.Deployment.Build;

[ValueObject(typeof(string))]
[Instance("NoError", "")]
[Instance("GeneralQueryFailed", "GeneralQueryFailed")]
[Instance("QueryAppNotFound", "QueryAppNotFound")]
[Instance("QueryFileNotFound", "QueryFileNotFound")]
[Instance("GerneralCommandError", "GerneralCommandError")]
[Instance("CommandErrorRegisterRepository", "CommandErrorRegisterRepository")]
[Instance("CommandDuplicateApp", "CommandDuplicateApp")]
[Instance("CommandAppNotFound", "CommandAppNotFound")]
[Instance("GernalBuildError", "GernalBuildError")]
[Instance("BuildDotnetNotFound", "BuildDotnetNotFound")]
[Instance("BuildDotNetFailed", "BuildDotNetFailed")]
[Instance("BuildProjectNotFound", "BuildProjectNotFound")]
#pragma warning disable MA0097
public readonly partial struct DeploymentErrorCode : IErrorConvertable
{
    Error IErrorConvertable.ToError() => Value;
}
#pragma warning restore MA0097
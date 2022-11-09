﻿using Vogen;

namespace Tauron.Application.Master.Commands.Deployment.Build;

[ValueObject(typeof(string))]
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
public readonly partial struct DeploymentErrorCode { }
using Vogen;

namespace Tauron.Application.Master.Commands.Deployment.Build;

[ValueObject(typeof(string))]
[Instance("RegisterRepository", "RegisterRepository")]
[Instance("BuildStart", "BuildStart")]
[Instance("BuildKilling", "BuildKilling")]
[Instance("BuildCompled", "BuildCompled")]
[Instance("BuildExtractingRepository", "BuildExtractingRepository")]
[Instance("BuildRunBuilding", "BuildRunBuilding")]
[Instance("BuildTryFindProject", "BuildTryFindProject")]
public readonly partial struct DeploymentMessage { }
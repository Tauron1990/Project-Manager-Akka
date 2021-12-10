using JetBrains.Annotations;
using Tauron.Application.Master.Commands.Deployment.Build.Data;

namespace Tauron.Application.Master.Commands.Deployment.Build.Querys;

[PublicAPI]
public sealed record QueryApps() : DeploymentQueryBase<QueryApps, AppList>("All");
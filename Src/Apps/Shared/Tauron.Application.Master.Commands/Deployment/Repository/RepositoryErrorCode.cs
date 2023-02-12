using Tauron.Operations;
using Vogen;

namespace Tauron.Application.Master.Commands.Deployment.Repository;

[ValueObject(typeof(string))]
[Instance("DuplicateRepository", "DuplicateRepository")]
[Instance("GithubNoRepoFound", "GithubNoRepoFound")]
[Instance("InvalidRepoName", "InvalidRepoName")]
[Instance("DatabaseNoRepoFound", "DatabaseNoRepoFound")]
#pragma warning disable MA0097
public readonly partial struct RepositoryErrorCode : IErrorConvertable
{
    Error IErrorConvertable.ToError() => Value;
}
#pragma warning restore MA0097
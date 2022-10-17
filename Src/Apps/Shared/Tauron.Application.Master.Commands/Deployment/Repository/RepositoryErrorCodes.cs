using Vogen;

namespace Tauron.Application.Master.Commands.Deployment.Repository;

[ValueObject(typeof(string))]
[Instance("DuplicateRepository", "DuplicateRepository")]
[Instance("GithubNoRepoFound", "GithubNoRepoFound")]
[Instance("InvalidRepoName", "InvalidRepoName")]
[Instance("DatabaseNoRepoFound", "DatabaseNoRepoFound")]
public readonly partial struct RepositoryErrorCode
{
    
}
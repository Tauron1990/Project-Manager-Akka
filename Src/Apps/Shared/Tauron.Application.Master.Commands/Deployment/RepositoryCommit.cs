using Vogen;

namespace Tauron.Application.Master.Commands.Deployment;

[ValueObject(typeof(string))]
[Instance("Empty", "")]
#pragma warning disable MA0097
public readonly partial struct RepositoryCommit { }
#pragma warning restore MA0097
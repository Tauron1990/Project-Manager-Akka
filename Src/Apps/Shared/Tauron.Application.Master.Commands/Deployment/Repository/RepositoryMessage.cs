using Vogen;

namespace Tauron.Application.Master.Commands.Deployment.Repository;

[ValueObject(typeof(string))]
[Instance("GetRepo", "GetRepo")]
[Instance("UpdateRepository", "UpdateRepository")]
[Instance("DownloadRepository", "DownloadRepository")]
[Instance("GetRepositoryFromDatabase", "GetRepositoryFromDatabase")]
[Instance("ExtractRepository", "ExtractRepository")]
[Instance("CompressRepository", "CompressRepository")]
[Instance("UploadRepositoryToDatabase", "UploadRepositoryToDatabase")]
#pragma warning disable MA0097
public readonly partial struct RepositoryMessage { }
#pragma warning restore MA0097
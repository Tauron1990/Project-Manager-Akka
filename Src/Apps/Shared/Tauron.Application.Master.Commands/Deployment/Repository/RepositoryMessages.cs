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
public readonly partial struct RepositoryMessage { }
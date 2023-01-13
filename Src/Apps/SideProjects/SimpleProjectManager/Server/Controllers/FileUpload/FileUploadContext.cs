using System.Collections.Concurrent;
using SimpleProjectManager.Shared;

namespace SimpleProjectManager.Server.Controllers.FileUpload;

public sealed record FileUploadContext(UploadFiles Files, ConcurrentBag<ProjectFileId> Ids);
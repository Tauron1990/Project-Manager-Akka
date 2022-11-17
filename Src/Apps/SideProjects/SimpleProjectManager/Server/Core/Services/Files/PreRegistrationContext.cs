using SimpleProjectManager.Shared;

namespace SimpleProjectManager.Server.Core.Services;

public sealed record PreRegistrationContext(Func<Stream> ToRegister, ProjectFileId FileId, string FileName, string JobName);
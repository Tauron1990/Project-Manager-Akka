using Tauron.Operations;

namespace SimpleProjectManager.Shared;

public sealed record AttachResult(SimpleResult FailMessage, bool IsNew);
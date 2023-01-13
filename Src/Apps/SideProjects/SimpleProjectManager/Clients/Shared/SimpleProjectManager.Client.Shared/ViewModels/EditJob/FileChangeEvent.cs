using System.Collections.Immutable;
using SimpleProjectManager.Client.Shared.Data.Files;

namespace SimpleProjectManager.Client.Shared.ViewModels.EditJob;

public sealed record FileChangeEvent(ImmutableList<IFileReference> Files);
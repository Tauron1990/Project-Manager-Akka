using System;
using System.Collections.Immutable;

namespace Tauron.Application.SoftwareRepo.Data;

public sealed record ApplicationEntry(
    string Name, Version Last, long Id, ImmutableList<DownloadEntry> Downloads,
    string RepositoryName, string BranchName);
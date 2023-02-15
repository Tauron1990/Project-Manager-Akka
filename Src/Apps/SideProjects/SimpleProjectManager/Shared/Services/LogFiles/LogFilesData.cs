﻿using System.Collections.Immutable;

namespace SimpleProjectManager.Shared.Services.LogFiles;

public sealed record LogFilesData(ImmutableList<LogFileEntry> Entries);
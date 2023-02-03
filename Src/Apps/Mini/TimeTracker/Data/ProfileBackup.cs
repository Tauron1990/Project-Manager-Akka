using System;
using System.Collections.Immutable;

namespace TimeTracker.Data;

public sealed record ProfileBackup(DateTime Data, ImmutableList<ProfileEntry> Entrys, ProfileData CurrentConfiguration);
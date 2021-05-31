using System;
using System.Collections.Immutable;
using Newtonsoft.Json;

namespace TimeTracker.Data
{
    public sealed record ProfileData(string FileName, int MonthHours, int MinusShortTimeHours, int AllHours, ImmutableList<ProfileEntry> Entries, DateTime CurrentMonth)
    {
        [JsonIgnore]
        public bool IsProcessable => !string.IsNullOrWhiteSpace(FileName);
    };

    public sealed record ProfileEntry(DateTime Date, TimeSpan? Start, TimeSpan? Finish);

    public sealed record ProfileBackup(DateTime Data, ImmutableList<ProfileEntry> Entrys);
}
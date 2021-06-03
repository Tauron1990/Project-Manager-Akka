using System;
using System.Collections.Immutable;
using Newtonsoft.Json;

namespace TimeTracker.Data
{
    public sealed record ProfileData(string FileName, int MonthHours, int MinusShortTimeHours, int AllHours, ImmutableDictionary<DateTime, ProfileEntry> Entries, DateTime CurrentMonth, 
        double WeekendMultiplikator = 0, double HoöidayMultiplicator = 0, bool HolidaysSet = false)
    {
        [JsonIgnore]
        public bool IsProcessable => !string.IsNullOrWhiteSpace(FileName);
    };

    public sealed record ProfileEntry(DateTime Date, TimeSpan? Start, TimeSpan? Finish, bool IsHoliday);

    public sealed record ProfileBackup(DateTime Data, ImmutableList<ProfileEntry> Entrys);
}
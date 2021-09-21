using System;
using System.Collections.Immutable;
using Newtonsoft.Json;
using TimeTracker.Managers;

namespace TimeTracker.Data
{
    public sealed record ProfileData(
        string FileName, int MonthHours, int MinusShortTimeHours, int AllHours, ImmutableDictionary<DateTime, ProfileEntry> Entries, DateTime CurrentMonth,
        ImmutableList<HourMultiplicator> Multiplicators, bool HolidaysSet, int DailyHours)
    {
        [JsonIgnore] public bool IsProcessable => !string.IsNullOrWhiteSpace(FileName);

        public static ProfileData New(string fileName, SystemClock clock)
            => new(fileName, 0, 0, 0, ImmutableDictionary<DateTime, ProfileEntry>.Empty, clock.NowDate, ImmutableList<HourMultiplicator>.Empty, HolidaysSet: false, 0);
    }

    public sealed record ProfileEntry(DateTime Date, TimeSpan? Start, TimeSpan? Finish, DayType DayType);

    public sealed record ProfileBackup(DateTime Data, ImmutableList<ProfileEntry> Entrys, ProfileData CurrentConfiguration);
}
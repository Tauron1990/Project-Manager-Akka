using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using DynamicData.Aggregation;
using DynamicData.Alias;
using Tauron;
using TimeTracker.Data;

namespace TimeTracker.Managers
{
    public class CalculationManager
    {
        private readonly SystemClock _clock;

        public CalculationManager(SystemClock clock, ProfileManager profileManager)
        {
            _clock = clock;

            var updateTrigger = Observable.Interval(TimeSpan.FromSeconds(30)).Select(_ => clock.NowDate)
               .StartWith(clock.NowDate)
               .Scan(
                    new TimeContainer(clock.NowDate, clock.NowDate, NeedUpdate: true),
                    (container, time) => container with
                                         {
                                             Old = container.New,
                                             New = time,
                                             NeedUpdate = container.New < time
                                         })
               .Where(c => c.NeedUpdate);

            var database = profileManager.ProcessableData.DistinctUntilChanged(ChangeToken.Get, new ChangeTokenComparer());

            var datastart = database.InvalidateWhen(updateTrigger).Isonlate();


            AllHours = datastart
               .SelectMany(
                    _ => profileManager.ConnectCache()
                       .Delay(TimeSpan.FromSeconds(1))
                       .TakeUntil(datastart)
                       .SelectMany(pe => profileManager.ProcessableData.Take(1).Select(pd => new EntryPair(pe, pd)).ToEnumerable(), ent => ent.Entry.Date)
                       .Where(
                            ep => ep.Data.CurrentMonth.Month == ep.Entry.Date.Month
                               && ep.Data.CurrentMonth.Year == ep.Entry.Date.Year
                               && ep.Entry.Date.Day <= clock.NowDay)
                       .ForAggregation().Sum(e => CalculateEntryTime(e.Entry, e.Data).TotalHours)
                       .Select(TimeSpan.FromHours))
               .Isonlate();

            CalculationResult = (from hours in AllHours
                                 from data in profileManager.ProcessableData.Take(1)
                                 select hours > TimeSpan.Zero && data.MonthHours > 0
                                     ? Calc(data.CurrentMonth, hours.TotalHours, data.MinusShortTimeHours, data.DailyHours)
                                     : new CalculationResult(MonthState.Minus, 0))
               .Isonlate();
        }

        public IObservable<TimeSpan> AllHours { get; }

        public IObservable<CalculationResult> CalculationResult { get; }


        public static IObservable<TimeSpan> GetEntryHourCalculator(ProfileEntry entry, IObservable<ProfileData> dataSource)
            => from data in dataSource
               select CalculateEntryTime(entry, data);

        private CalculationResult Calc(DateTime currentMonth, double allHouers, int maxShort, int dailyHours)
        {
            var currentDays = (double)_clock.NowDaysCurrentMonth(currentMonth);
            var currentTarget = currentDays * dailyHours;

            var remaining = allHouers - currentTarget;

            if (remaining > 0)
                return new CalculationResult(MonthState.Ok, (int)remaining);

            return maxShort > 0 && remaining > maxShort * -1
                ? new CalculationResult(MonthState.Short, (int)remaining)
                : new CalculationResult(MonthState.Minus, (int)remaining);
        }

        private static TimeSpan CalculateEntryTime(ProfileEntry entry, ProfileData data)
        {
            return entry.DayType switch
            {
                DayType.Normal when EntryValid(entry) => entry.Finish!.Value - entry.Start!.Value,
                DayType.Vacation => TimeSpan.FromHours(data.DailyHours),
                DayType.Holiday when EntryValid(entry) => entry.Finish!.Value - entry.Start!.Value + TimeSpan.FromHours(data.DailyHours),
                DayType.Holiday when entry.Date.DayOfWeek != DayOfWeek.Sunday || entry.Date.DayOfWeek != DayOfWeek.Saturday => TimeSpan.FromHours(data.DailyHours),
                _ => TimeSpan.Zero
            };

            static bool EntryValid(ProfileEntry entry)
                => entry.Start != null && entry.Finish != null && entry.Finish > entry.Start;
        }

        //public static int CalculateShortTimeHours(int mouthHours)
        //{
        //    var multi = mouthHours / 100d;
        //    var minus = 10 * multi;

        //    return (int)Math.Round(minus, 0, MidpointRounding.ToPositiveInfinity);
        //}

        private record EntryPair(ProfileEntry Entry, ProfileData Data);

        private record TimeContainer(DateTime Old, DateTime New, bool NeedUpdate);

        private record ChangeToken(string File, int One, int Two, int Three, object Four)
        {
            internal static ChangeToken Get(ProfileData data)
                => new(data.FileName, data.MonthHours, data.MinusShortTimeHours, data.DailyHours, data.Multiplicators);
        }

        private sealed class ChangeTokenComparer : IEqualityComparer<ChangeToken>
        {
            public bool Equals(ChangeToken? x, ChangeToken? y)
            {
                if (ReferenceEquals(x, y)) return true;
                if (ReferenceEquals(x, null)) return false;
                if (ReferenceEquals(y, null)) return false;
                if (x.GetType() != y.GetType()) return false;

                return string.Equals(x.File, y.File, StringComparison.Ordinal) && x.One == y.One && x.Two == y.Two && x.Three == y.Three && x.Four.Equals(y.Four);
            }

            public int GetHashCode(ChangeToken obj) => HashCode.Combine(obj.File, obj.One, obj.Two, obj.Three, obj.Four);
        }
    }
}
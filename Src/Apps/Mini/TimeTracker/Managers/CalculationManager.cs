using System;
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

        public IObservable<TimeSpan> AllHours { get; }

        public IObservable<CalculationResult> CalculationResult { get; }

        public CalculationManager(ProfileManager profileManager, SystemClock clock)
        {
            _clock = clock;
            AllHours = profileManager.ConnectCache()
                                     .SelectMany(pe => profileManager.ProcessableData.Take(1).Select(pd => new EntryPair(pe, pd)).ToEnumerable(), ent => ent.Entry.Date)
                                     .ForAggregation()
                                     .Sum(e => CalculateEntryTime(e.Entry, e.Data).TotalHours)
                                     .Select(TimeSpan.FromHours)
                                     .Isonlate();

            CalculationResult = (from hours in AllHours
                                 from data in profileManager.ProcessableData.Take(1)
                                 where hours > TimeSpan.Zero && data.MonthHours > 0
                                 select Calc(data.CurrentMonth, hours.Hours, data.MonthHours, data.MinusShortTimeHours))
                               .Publish().RefCount();
        }


        public IObservable<TimeSpan> GetEntryHourCalculator(ProfileEntry entry, IObservable<ProfileData> dataSource)
            => from data in dataSource
               select CalculateEntryTime(entry, data);

        private CalculationResult Calc(DateTime currentMonth, int allHouers, int targetHours, int maxShort)
        {
            var targetDays = (double)_clock.DaysInCurrentMonth(currentMonth);
            var targetCurrent = targetHours / (targetDays - _clock.NowDay);
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (targetCurrent == double.PositiveInfinity)
                targetCurrent = targetHours;

            var remaining = allHouers - targetCurrent;
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
                DayType.Holiday when EntryValid(entry) => (entry.Finish!.Value - entry.Start!.Value) + TimeSpan.FromHours(data.DailyHours),
                DayType.Holiday => TimeSpan.FromHours(data.DailyHours),
                _ => TimeSpan.Zero
            };

            static bool EntryValid(ProfileEntry entry)
                => entry.Start != null && entry.Finish != null && entry.Finish > entry.Start;
        }

        public static int CalculateShortTimeHours(int mouthHours)
        {
            var multi = mouthHours / 100d;
            var minus = 10 * multi;

            return (int)Math.Round(minus, 0, MidpointRounding.ToPositiveInfinity);
        }

        private record EntryPair(ProfileEntry Entry, ProfileData Data);
    }

    public sealed record CalculationResult(MonthState MonthState, int Remaining);
}
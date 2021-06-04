using System;
using TimeTracker.Data;

namespace TimeTracker.Managers
{
    public class CalculationManager
    {
        public IObservable<TimeSpan> GetEntryHourCalculator(TimeSpan? start, TimeSpan? end, IObservable<ProfileData> dataSource)
        {

        }

        public static int CalculateShortTimeHours(int mouthHours)
        {
            var multi = mouthHours / 100d;
            var minus = 10 * multi;

            return (int)Math.Round(minus, 0, MidpointRounding.ToPositiveInfinity);
        }
    }
}
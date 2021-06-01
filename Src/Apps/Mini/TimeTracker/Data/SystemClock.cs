using System;
using Microsoft.VisualBasic.Devices;

namespace TimeTracker.Data
{
    public abstract class SystemClock
    {
        private static readonly SystemClock Inst = new Test();

        public static DateTime NowDate => Inst.NowDateImpl;

        public static TimeSpan NowTime => Inst.NowTimeImpl;

        public static double NowDay => Inst.NewDayImpl;

        public static int DaysInCurrentMonth(DateTime month) => Inst.DaysInCurrentMonthImpl(month);

        protected abstract double NewDayImpl { get; }

        protected abstract DateTime NowDateImpl { get; }

        protected abstract TimeSpan NowTimeImpl { get; }

        protected abstract int DaysInCurrentMonthImpl(DateTime month);

        private sealed class Test : SystemClock
        {
            private DateTime _current = DateTime.UtcNow - TimeSpan.FromDays(1);
            private int _multipler;


            private DateTime Get()
            {
                _current = _current.AddDays(1);
                _multipler = 0;
                return _current;
            }

            private TimeSpan GetTime()
            {
                _multipler++;

                return _current.TimeOfDay * _multipler;
            }

            protected override double NewDayImpl => _current.Day;
            protected override DateTime NowDateImpl => Get().Date;
            protected override TimeSpan NowTimeImpl => GetTime();
            protected override int DaysInCurrentMonthImpl(DateTime month) => DateTime.DaysInMonth(month.Year, month.Month);
        }

        private sealed class Actual : SystemClock
        {
            protected override double NewDayImpl => DateTime.UtcNow.Day;
            protected override DateTime NowDateImpl => DateTime.UtcNow.Date;
            protected override TimeSpan NowTimeImpl => DateTime.UtcNow.TimeOfDay;
            protected override int DaysInCurrentMonthImpl(DateTime month) => DateTime.DaysInMonth(month.Year, month.Month);
        }
    }
}
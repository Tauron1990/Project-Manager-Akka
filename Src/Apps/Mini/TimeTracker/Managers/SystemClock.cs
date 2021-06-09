using System;
using System.Diagnostics;
using System.Linq;

namespace TimeTracker.Managers
{
    public abstract class SystemClock
    {
        public static readonly SystemClock Inst = new Actual();

        //public static DateTime NowDate => Inst.NowDate;

        //public static TimeSpan NowTime => Inst.NowTime;

        //public static double NowDay => Inst.NowDay;

        //public static int DaysInCurrentMonth(DateTime month) => Inst.DaysInCurrentMonth(month);

        public abstract int NowDay { get; }

        public abstract DateTime NowDate { get; }

        public abstract TimeSpan NowTime { get; }

        public abstract int DaysInCurrentMonth(DateTime month);

        public abstract int NowDaysCurrentMonth(DateTime month);

        //private sealed class Test : SystemClock
        //{
        //    private DateTime _current = DateTime.UtcNow - TimeSpan.FromDays(1);
        //    private int _multipler;


        //    private DateTime Get()
        //    {
        //        _current = _current.AddDays(1);
        //        _multipler = 0;
        //        return _current;
        //    }

        //    private TimeSpan GetTime()
        //    {
        //        _multipler++;

        //        return _current.TimeOfDay * _multipler;
        //    }

        //    protected override double NowDay => _current.Day;
        //    protected override DateTime NowDate => Get().Date;
        //    protected override TimeSpan NowTime => GetTime();
        //    protected override int DaysInCurrentMonth(DateTime month) => DateTime.DaysInMonth(month.Year, month.Month);
        //}

        [DebuggerStepThrough]
        public static bool IsWeekDay(DateTime dateTime)
            => dateTime.DayOfWeek != DayOfWeek.Saturday && dateTime.DayOfWeek != DayOfWeek.Sunday;

        private sealed class Actual : SystemClock
        {
            public override int NowDay => DateTime.Now.Day;
            public override DateTime NowDate => DateTime.Now.Date;
            public override TimeSpan NowTime => DateTime.Now.TimeOfDay;

            [DebuggerStepThrough]
            public override int DaysInCurrentMonth(DateTime month)
            {
                var count = 0;
                var target = DateTime.DaysInMonth(month.Year, month.Day);

                for (int i = 1; i < target; i++)
                {
                    if (IsWeekDay(new DateTime(month.Year, month.Month, i)))
                        count++;
                }

                return count;
            }

            [DebuggerStepThrough]
            public override int NowDaysCurrentMonth(DateTime month)
            {
                var count = 0;
                var target = NowDay;

                for (int i = 1; i < target; i++)
                {
                    if (IsWeekDay(new DateTime(month.Year, month.Month, i)))
                        count++;
                }

                return count;
            }
        }
    }
}
using System;

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

        private sealed class Actual : SystemClock
        {
            public override int NowDay => DateTime.UtcNow.Day;
            public override DateTime NowDate => DateTime.UtcNow.Date;
            public override TimeSpan NowTime => DateTime.UtcNow.TimeOfDay;
            public override int DaysInCurrentMonth(DateTime month) => DateTime.DaysInMonth(month.Year, month.Month);
        }
    }
}
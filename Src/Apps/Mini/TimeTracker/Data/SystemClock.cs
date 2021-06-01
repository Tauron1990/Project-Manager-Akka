using System;

namespace TimeTracker.Data
{
    public abstract class SystemClock
    {
        private static readonly SystemClock Inst = new Test();

        public static DateTime NowDate => Inst.NowDateImpl;

        public static TimeSpan NowTime => Inst.NowTimeImpl;

        public abstract DateTime NowDateImpl { get; }

        public abstract TimeSpan NowTimeImpl { get; }

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

            public override DateTime NowDateImpl => Get().Date;
            public override TimeSpan NowTimeImpl => GetTime();
        }

        private sealed class Actual : SystemClock
        {
            public override DateTime NowDateImpl => DateTime.UtcNow.Date;
            public override TimeSpan NowTimeImpl => DateTime.UtcNow.TimeOfDay;
        }
    }
}
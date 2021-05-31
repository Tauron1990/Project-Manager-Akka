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

            private DateTime Get()
            {
                _current = _current.AddDays(1);
                return _current;
            }

            public override DateTime NowDateImpl => Get().Date;
            public override TimeSpan NowTimeImpl => _current.TimeOfDay;
        }

        private sealed class Actual : SystemClock
        {
            public override DateTime NowDateImpl => DateTime.UtcNow.Date;
            public override TimeSpan NowTimeImpl => DateTime.UtcNow.TimeOfDay;
        }
    }
}
using System;
using DynamicData.Kernel;

namespace TimeTracker.Data
{
    public sealed record ComeParameter(DateTime Time, bool Override)
    {
        public bool CanCreate(Func<DateTime, Optional<ProfileEntry>> lookup)
        {
            if (Override) return true;

            return !lookup(Time).HasValue;
        }
    }
}
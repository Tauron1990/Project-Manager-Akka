using System;

namespace Tauron.Application.AkkaNode.Services.CleanUp
{
    public sealed record CleanUpTime(string Id, TimeSpan Interval, DateTime Last)
    {
        public CleanUpTime()
            : this(string.Empty, TimeSpan.Zero, DateTime.MinValue)
        {
            
        }
    }
}
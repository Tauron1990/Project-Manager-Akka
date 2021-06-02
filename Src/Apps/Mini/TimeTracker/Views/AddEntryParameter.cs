using System;
using System.Collections.Generic;

namespace TimeTracker.Views
{
    public sealed record AddEntryParameter(IEnumerable<int> BlockedDays, DateTime CurrentMonth);
}
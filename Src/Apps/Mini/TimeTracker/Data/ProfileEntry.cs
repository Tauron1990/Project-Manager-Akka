using System;

namespace TimeTracker.Data;

public sealed record ProfileEntry(DateTime Date, TimeSpan? Start, TimeSpan? Finish, DayType DayType);
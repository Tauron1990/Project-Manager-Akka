﻿using System;

namespace TimeTracker.Data
{
    public sealed record HourMultiplicator(MultiplicatorType MultiplicatorType, double Multiplicator, DayOfWeek TargetDay);
}
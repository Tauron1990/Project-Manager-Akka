using System;

namespace Tauron.Application.AkkaNode.Services.CleanUp
{
    public sealed record CleanUpTime(string Id, TimeSpan Interval, DateTime Last);
}
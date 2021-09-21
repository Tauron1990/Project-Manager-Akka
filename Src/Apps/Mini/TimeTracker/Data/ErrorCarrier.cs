using System;
using Tauron.Application;

namespace TimeTracker.Data
{
    public sealed class ErrorCarrier : AggregateEvent<Exception> { }
}
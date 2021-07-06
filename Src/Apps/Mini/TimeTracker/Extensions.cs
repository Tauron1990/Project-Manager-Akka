using System;
using System.IO;
using Tauron.Application;
using TimeTracker.Data;

namespace TimeTracker
{
    public static class Extensions
    {
        public static string AppData(this ITauronEnviroment enviroment)
            => Path.Combine(enviroment.LocalApplicationData, "Time-Tracker");

        public static void ReportError(this IEventAggregator aggregator, Exception exception)
            => aggregator.GetEvent<ErrorCarrier, Exception>().Publish(exception);

        public static IObservable<Exception> ConsumeErrors(this IEventAggregator aggregator)
            => aggregator.GetEvent<ErrorCarrier, Exception>().Get();
    }
}
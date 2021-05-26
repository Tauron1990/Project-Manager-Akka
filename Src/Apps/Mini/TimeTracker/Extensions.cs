using System.IO;
using Tauron.Application;

namespace TimeTracker
{
    public static class Extensions
    {
        public static string AppData(this ITauronEnviroment enviroment)
            => Path.Combine(enviroment.LocalApplicationData, "Time-Tracker");
    }
}
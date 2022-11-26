using System.Diagnostics;
using JetBrains.Annotations;

namespace Tauron.Servicemnager.Networking.Ipc;

[PublicAPI]
internal class SspinWait
{
    private const long TestIterations = 10000;
    private static long _iterationsInMs; //300K

    private int _currentIter;
    private SpinWait _spinwt;
    private const int WaitIter = 5; //Start waiting from 10 iterations then every new iteration doubling. 100 times , then spinwt.SpinOnce

    internal void Spin()
    {
        if(_currentIter > _iterationsInMs)
        {
            _spinwt.SpinOnce();

            return;
        }

        Thread.SpinWait(WaitIter);
        _currentIter += WaitIter;
    }

    internal void Clear()
        => _currentIter = 0;

    /// <summary>
    ///     Runs onec in the beginning
    /// </summary>
    internal static void Measure()
    {

        var sw = new Stopwatch();
        sw.Start();
        Thread.Sleep(1);
        sw.Stop();

        long tksInMs = sw.ElapsedTicks;
        sw.Reset();
        sw.Start();
        Thread.SpinWait((int)TestIterations);
        sw.Stop();

        //TimeSpan.TicksPerMillisecond
        _iterationsInMs = tksInMs / sw.ElapsedTicks * TestIterations;
    }
}
using BenchmarkDotNet.Running;

namespace TestApp;

#pragma warning disable GU0011

internal class Program
{
    private static void Main()
    {
        BenchmarkRunner.Run<Benchmarks>();
    }

}
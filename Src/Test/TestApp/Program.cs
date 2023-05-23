using System;
using BenchmarkDotNet.Running;
using FluentResults;

namespace TestApp;

#pragma warning disable GU0011

internal class Program
{
    private static void Main()
    {
        BenchmarkRunner.Run<Benchmarks>();
    }

}
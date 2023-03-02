using System;
using BenchmarkDotNet.Running;
using LanguageExt;
using LanguageExt.Pipes;

using LanguageExt;
using static LanguageExt.Prelude;
using static LanguageExt.List;
using LanguageExt.ClassInstances;
using LanguageExt.ClassInstances.Const;
using LanguageExt.ClassInstances.Pred;
using LanguageExt.SomeHelp;
using LanguageExt.TypeClasses;

namespace TestApp;

#pragma warning disable GU0011

internal class Program
{
    private static void Main()
    {
        //BenchmarkRunner.Run<Benchmarks>();
        
        var result =
            from initial in Some(10)
            from add1 in Add(initial, 10)
            let add2 = add1 + initial
            from add3 in Add(add2, 10)
            select add3 + add2 + add1 + initial;

        result.Iter(Console.WriteLine);
    }

    public static Option<int> Add(int value, int value2)
        => Some(value + value2);
}
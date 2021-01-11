using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace AkkaTest
{
    internal static class Program
    {
        private static void Main()
        {
            using var sub = new Subject<string>();

            sub.Select(int.Parse).Subscribe(Console.WriteLine);

            sub.Select<string, int>(_ => throw new InvalidOperationException()).Subscribe(Console.WriteLine, e => Console.WriteLine(e));

            sub.OnNext("123");
        }
    }
}
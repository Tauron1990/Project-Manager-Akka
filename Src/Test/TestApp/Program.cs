using System;
using System.Collections.Immutable;
using System.Linq;
using Be.Vlaanderen.Basisregisters.Generators.Guid;

namespace TestApp
{
    static class Program
    {
        static void Main()
        {
            var guidBase = Guid.Parse("A8D7D183-928B-439B-8134-580873A6E03F");

            var result = Enumerable.Range(2_000, 3_000)
               .AsParallel()
               .Aggregate(
                    ImmutableList<string>.Empty,
                    (list, year) => list.AddRange(
                        Enumerable.Range(0, 20_000)
                           .Select(number => Deterministic.Create(guidBase, $"BM{year}_{number}").ToString("D"))),
                    (total, part) => total.AddRange(part),
                    l => l);

            var result2 = result.AsParallel().Distinct(StringComparer.Ordinal).Count();

            if (result.Count != result2)
            {
                Console.WriteLine("Duplicates Found");
                Console.WriteLine($"All: {result.Count} -- Duplicates: {result.Count - result2}");
            }
            else
                Console.WriteLine($"No Duplicates: {result.Count}");
        }
    }
}
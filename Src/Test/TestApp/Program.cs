using LanguageExt;
using LanguageExt.Pipes;
using LanguageExt.Sys;
using LanguageExt.Sys.IO;
using LanguageExt.Sys.Live;

namespace TestApp;

#pragma warning disable GU0011

internal class Program
{
    private static void Main()
    {
        //BenchmarkRunner.Run<Benchmarks>();

        var open = File<Runtime>.openRead("Test.txt");
        var pipe = Stream<Runtime>.read(1000);

        var consumer =
            from data in Consumer.awaiting<Runtime, SeqLoan<byte>>()
            from ebc in Enc<Runtime>.encoding
            select ebc.GetString(data.ToSpan());


        var combine = open | pipe;

    }
}
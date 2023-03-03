

using System;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using LanguageExt;
using LanguageExt.Pipes;
using LanguageExt.Sys;
using LanguageExt.Sys.IO;
using LanguageExt.SysX.Live;

namespace TestApp;

#pragma warning disable GU0011

internal class Program
{
    private static async  Task Main()
    {
        //BenchmarkRunner.Run<Benchmarks>();

        var open = File<Runtime>.openRead("Test.txt");
        
        var reader = Stream<Runtime>.read(1000);
        
        var decoder =
            from data in Proxy.awaiting<SeqLoan<byte>>()
            from _ in Proxy.yield(Encoding.UTF8.GetString(data.ToReadOnlySpan()))
            select Unit.Default;

        var consumer =
            from data in Proxy.awaiting<string>()
            from _ in Console<Runtime>.writeLine(data)
            select Unit.Default;
            
            

        var test = open | reader | decoder.Interpret<Runtime>() | consumer;

        var main =
            from _ in test
            from res in Console<Runtime>.readKey
            select res;


        var fin = await main.Run(Runtime.New());

        fin.ThrowIfFail();


        // var consumer =
        //     from data in Consumer.awaiting<Runtime, SeqLoan<byte>>()
        //     from ebc in Enc<Runtime>.encoding
        //     select ebc.GetString(data.ToSpan());
    }
}
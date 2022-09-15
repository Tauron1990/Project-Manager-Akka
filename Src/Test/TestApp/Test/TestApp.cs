using System;
using System.Threading.Channels;
using System.Threading.Tasks;
using Stl;
using Tauron.ObservableExt;

namespace TestApp.Test;

public sealed record TestData(string[] Data)
{
    public static Option<TestData2> Create(params string[] data)
        => data.Length == 0 ? default(Option<TestData2>) : new TestData2(new TestData(data));
}

public sealed record TestData2(TestData Data)
{
    public static implicit operator TestData2(TestData data)
        => new(data);

    public static explicit operator TestData(TestData2 data2)
        => data2.Data;
}

public static class TestApp
{
    
    public static async Task Run()
    {

        /*var channel = Channel.CreateUnbounded<Option<TestData2>>();

        var newReader = (
            from opt in channel
            from actualData in data.Data
            where actualData.Contains('l')
            select actualData
        );

        var runner = Printer(newReader);

        var writer = channel.Writer;

        await writer.WriteAsync(TestData.Create());
        await writer.WriteAsync(TestData.Create("Hallo"));
        await writer.WriteAsync(TestData.Create("Welt"));
        await writer.WriteAsync(TestData.Create("Hallo Welt"));
        await writer.WriteAsync(TestData.Create());
        
        writer.Complete();
        
        await runner;*/
    }

    private static async Task Printer(ChannelReader<string> reader)
    {
        await foreach(var element in reader.ReadAllAsync())
            Console.WriteLine(element);
    }
}
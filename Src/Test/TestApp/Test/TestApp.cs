using System;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace TestApp.Test;

public sealed record TestData(string[] Data)
{
    public static TestData Create(params string[] data) 
        => new(data);
}

public static class TestApp
{
    
    public static async Task Run()
    {
        var channel = Channel.CreateUnbounded<TestData>();

        var newReader = (
            from data in channel
            from actualData in data.Data
            where actualData.Contains('l')
            select actualData
        );

        var runner = Printer(newReader);

        var writer = channel.Writer;

        await writer.WriteAsync(TestData.Create("Hallo"));
        await writer.WriteAsync(TestData.Create("Welt"));
        await writer.WriteAsync(TestData.Create("Hallo Welt"));
        
        writer.Complete();
        
        await runner;
    }

    private static async Task Printer(ChannelReader<string> reader)
    {
        await foreach(var element in reader.ReadAllAsync())
            Console.WriteLine(element);
    }
}
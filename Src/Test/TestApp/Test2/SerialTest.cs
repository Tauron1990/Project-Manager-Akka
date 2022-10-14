using System;
using System.IO;
using Hyperion;
using UnitsNet;
using UnitsNet.Units;

namespace TestApp.Test2;

public sealed record TestData(ToSerialize ToSerialize, Temperature Temperature);

public static class SerialTest
{
    public static void Run()
    {
        var test = new Serializer();

        using var mem = new MemoryStream();
        test.Serialize(new TestData(ToSerialize.From("Hallo Welt"), Temperature.FromDegreesCelsius(30)), mem);

        mem.Seek(0, SeekOrigin.Begin);
        var resut = test.Deserialize<TestData>(mem);

        string testText = resut.ToSerialize.Value;
        Console.WriteLine(testText);
    }
}
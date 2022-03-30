using System;
using System.Threading.Tasks;

namespace TestApp;

public abstract class TestClassBase
{
    protected TestClassBase()
    {
        // ReSharper disable once VirtualMemberCallInConstructor
        Console.WriteLine(GetMsg());
    }
    
    protected abstract string GetMsg();
}

// ReSharper disable once UnusedType.Global
public sealed class TestClass : TestClassBase
{
    public string TestProp { get; set; } = "Hallo Welt";
    protected override string GetMsg()
        => TestProp;
}

static class Program
{
    static Task Main()
    {

        
        Console.WriteLine();
        Console.WriteLine("Fertig...");
        Console.ReadKey();

        return Task.CompletedTask;
    }
}
using System;
using System.Threading.Tasks;

namespace TestApp;

public abstract class TestClassBase
{
    protected TestClassBase()
    {
        Console.WriteLine(GetMsg());
    }
    
    protected abstract string GetMsg();
}

public sealed class TestClass : TestClassBase
{
    public string TestProp { get; set; } = "Hallo Welt";
    protected override string GetMsg()
        => TestProp;
}

static class Program
{
    static async Task Main()
    {

        
        Console.WriteLine();
        Console.WriteLine("Fertig...");
        Console.ReadKey();
    }
}
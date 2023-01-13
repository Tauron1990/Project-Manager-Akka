using System.Threading.Tasks;
using TestApp.Test2;

namespace TestApp;

internal static class Program
{
    private static async Task Main()
        => SerialTest.Run();
}
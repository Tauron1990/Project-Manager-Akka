using System.Text;
using System.Linq;
using Servicemnager.Networking.Data;

namespace AkkaTest
{
    internal static class Program
    {
        

        private static void Main()
        {
            var formatter = new NetworkMessageFormatter();

            var output = formatter.WriteMessage(formatter.Create("TestMessage", Encoding.ASCII.GetBytes("Hallo Welt")));
            using var data = output.Message;

            var result = Encoding.ASCII.GetString(formatter.ReadMessage(data).Data);
        }
    }
}
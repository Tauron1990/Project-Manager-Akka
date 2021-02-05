using System;
using System.Text;
using tiesky.com;

namespace AkkaTest
{
    internal static class Program
    {
        private static TextIpc _master;

        private static void Main()
        {
            var id = Guid.NewGuid().ToString("N");

            _master = new TextIpc(id, false);


            try
            {
                _master.Send("Hallo Welt");

                Console.ReadKey();
            }
            finally
            {
                _master.Dispose();
            }
        }
    }

    internal class TextIpc : IDisposable
    {
        private static int _counter = 1;
        private readonly SharmIpc _ipc;

        private readonly string _name;

        public TextIpc(string name, bool slave)
        {
            if (slave)
            {
                _name = $"Slave {_counter}";
                _counter++;
            }
            else
            {
                _name = "Master";
            }

            _ipc = new SharmIpc(name, RemoteCallHandler,
                ExternalExceptionHandler: (s, exception) => Console.WriteLine(exception.ToString()));
        }

        public void Dispose() => _ipc.Dispose();

        private void RemoteCallHandler(ulong arg1, byte[] arg2)
        {
            Console.WriteLine($"{_name}: Message Id: {arg1} Message: {Encoding.UTF8.GetString(arg2)}");
        }

        public void Send(string message)
        {
            if (!_ipc.RemoteRequestWithoutResponse(Encoding.UTF8.GetBytes(message)))
                Console.WriteLine("Error on Send");
        }
    }
}
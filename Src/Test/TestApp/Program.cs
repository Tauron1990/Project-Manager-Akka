using System;
using System.Management;

namespace TestApp;

static class Program
{
    static void Main()
    {
        var q = new WqlEventQuery
                {
                    EventClassName = "Win32_ProcessStartTrace"
                };
        using var w = new ManagementEventWatcher(q);
        w.EventArrived += ProcessStartEventArrived;
        w.Start();
        Console.ReadLine(); // block main thread for test purposes

        static void ProcessStartEventArrived(object sender, EventArrivedEventArgs e)
        {
            foreach (PropertyData pd in e.NewEvent.Properties)
            {
                Console.WriteLine("\n============================= =========");
                Console.WriteLine("{0},{1},{2}", pd.Name, pd.Type, pd.Value);
            }
        }
    }
}
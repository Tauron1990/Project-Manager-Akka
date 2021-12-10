// See https://aka.ms/new-console-template for more information

//ProcessName,String,TestApp.exe
//ProcessID,UInt32,14044

using System.Collections;
using System.Diagnostics;
using System.Management;

var primaryArray = new BitArray(new []{ true, true, true, true, false, false, false, false });
var secondaryArray = new BitArray(new []{ false, false, false, false, true, true, true, true });
var data = new[] {0, 0};

primaryArray.CopyTo(data, 0);
secondaryArray.CopyTo(data, 1);

var q = new WqlEventQuery
        {
            EventClassName = "Win32_ProcessStartTrace"
        };
using var w = new ManagementEventWatcher(q);

try
{

    w.EventArrived += ProcessStartEventArrived;
    w.Start();
    Console.WriteLine("Zum Beenden Taste Drücken");
    Console.ReadLine(); // block main thread for test purposes
}
catch (Exception ex)
{
    Console.WriteLine("Schwerer Fehler");
    Console.WriteLine(ex);
}
finally
{
    w.Stop();
    w.Dispose();
}

void ProcessStartEventArrived(object sender, EventArrivedEventArgs e)
{
    var propertyData = e.NewEvent.Properties.OfType<PropertyData>().FirstOrDefault(d => d.Name == "ProcessID");
    if(propertyData is null) return;

    try
    {
        using var process = Process.GetProcessById((int)(uint)propertyData.Value);
        var name = process.ProcessName;
        if (name.Contains(@"Noyau_MGI.exe") || name.Contains(@"JETvarnish.exe"))
            process.ProcessorAffinity = (IntPtr)data[0];
        else
            process.ProcessorAffinity = (IntPtr)data[1];

        Console.WriteLine($"Affinity Settet: {name}");
    }
    catch (Exception exception)
    {
        Console.WriteLine("Fehler beim setzten der Process zugeörigkeit");
        Console.WriteLine(exception.Message);
    }
}
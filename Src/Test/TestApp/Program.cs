using System;
using System.Globalization;
using System.Threading.Tasks;

namespace TestApp;

internal static class Program
{
    private static void Main()
    {
        var test = "12:30:20:10";

        var resut = ToDateTime(test);
    }
    
    public static DateTime ToDateTime(string valueString)
    {
        var value = valueString.AsSpan();
        var date = DateTime.Now.Date;

        int hour = int.Parse(value[..2], NumberStyles.Any, CultureInfo.InvariantCulture);
        int minutes = int.Parse(value[3..5], NumberStyles.Any, CultureInfo.InvariantCulture);
        int secunds = int.Parse(value[6..8], NumberStyles.Any, CultureInfo.InvariantCulture);
        int mili = int.Parse(value[9..11], NumberStyles.Any, NumberFormatInfo.InvariantInfo);

        return date + new TimeSpan(0, hour, minutes, secunds, mili);
    }
}
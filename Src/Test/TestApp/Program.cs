using System;
using System.Globalization;
using System.IO;
using System.Text.Json;
using LinqToDB;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Tauron;
using Tauron.Application.Logging;

namespace TestApp;

internal class Program
{
    private static void Main()
    {
        var fac = new ServiceCollection();
        fac.AddLogging(b => b.ConfigDefaultLogging("Test App"));
        using ServiceProvider provider = fac.BuildServiceProvider();

        var logger = provider.GetRequiredService<ILogger<Program>>();
        logger.LogInformation("Start Logging Test");
        
        const string test = "12:30:20:10";
        logger.LogInformation("The Test String is {TestString} {TestString2}", test, "hallo");
        
        DateTime resut = ToDateTime(test);
        logger.LogInformation("The Result Date is {ResultDate}", resut);
        
        logger.LogError(new InvalidOperationException("Test Exception Instance"), "Test Messsage {For}", "Exception Message");
        
        logger.LogCritical("Test Messsage {For}", "Fatal Message");
    }

    private static DateTime ToDateTime(string valueString)
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
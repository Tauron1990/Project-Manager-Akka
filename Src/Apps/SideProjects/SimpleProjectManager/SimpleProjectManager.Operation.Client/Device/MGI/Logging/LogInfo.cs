using System.Globalization;

namespace SimpleProjectManager.Operation.Client.Device.MGI.Logging;

public sealed record LogInfo(DateTime TimeStamp, string Application, string Type, string Content, Command Command)
{
    public static LogInfo CreateError(string error) 
        => new(DateTime.Now, "KernelLogger-Proxy", "ERROR", error, Command.Log);

    public static LogInfo CreateSave() 
        => new(DateTime.Now, "KernelLogger-Proxy", "SAVE", "Saving Log", Command.Log);

    public static LogInfo CreateCommad(Command command, string info)
        => new LogInfo(DateTime.Now, "KernelLogger-Proxy", "COMMAND", info, command);

    public DateTime ToDateTime(string valueString)
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
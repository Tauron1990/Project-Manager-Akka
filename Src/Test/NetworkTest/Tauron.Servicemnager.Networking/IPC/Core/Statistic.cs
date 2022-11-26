using System.Globalization;
using System.Text;

namespace Tauron.Servicemnager.Networking.IPC.Core;

internal class Statistic
{
    private int _errorTotalBytesInQueue;
    private readonly ulong _reading = 0;
    private readonly int _readingMax = 0;
    private readonly ulong _readingTimes = 1;
    private long _readProcedureMax = -1;
    private DateTime _readProcedureMaxSetup = DateTime.MinValue;

    private DateTime _readProcedureStart = DateTime.MinValue;
    internal DateTime Ready2ReadSignalLastSetup = DateTime.MinValue;
    private ulong _ready2WriteSignalCalls;
    private long _ready2WriteSignalLast = -1;
    internal DateTime Ready2WriteSignalLastSetup = DateTime.MinValue;
    private long _ready2WriteSignalMax = -1;
    private DateTime _ready2WriteSignalMaxSetup = DateTime.MinValue;
    private DateTime _ready2WriteSignalStart = DateTime.MinValue;
    private int _timeouts;
    private DateTime _timeoutsLastSetup = DateTime.MinValue;
    private long _waitForReadMax = -1;
    private DateTime _waitForReadMaxSetup = DateTime.MinValue;

    private DateTime _waitForReadStart = DateTime.MinValue;
    private readonly ulong _writing = 0;
    private readonly int _writingMax = 0;
    private readonly ulong _writingTimes = 1;
    internal SharmIpc? Ipc = null;

    internal long TotalBytesInQueue = 0;


    internal void Start_WaitForRead_Signal()
        => _waitForReadStart = DateTime.UtcNow;

    internal void Stop_WaitForRead_Signal()
    {
        Ready2ReadSignalLastSetup = DateTime.UtcNow;

        if(_waitForReadStart == DateTime.MinValue)
            return;

        long t = DateTime.UtcNow.Subtract(_waitForReadStart).Ticks;
        if(_waitForReadMax < t)
        {
            _waitForReadMax = t;
            _waitForReadMaxSetup = DateTime.UtcNow;
        }
    }


    internal void Start_ReadProcedure_Signal()
        => _readProcedureStart = DateTime.UtcNow;

    internal void Stop_ReadProcedure_Signal()
    {
        long t = DateTime.UtcNow.Subtract(_readProcedureStart).Ticks;
        if(_readProcedureMax < t)
        {
            _readProcedureMax = t;
            _readProcedureMaxSetup = DateTime.UtcNow;
        }
    }

    internal void StartToWait_ReadyToWrite_Signal()
    {
        _ready2WriteSignalStart = DateTime.UtcNow;
        _ready2WriteSignalCalls++;
    }

    internal void StopToWait_ReadyToWrite_Signal()
    {
        Ready2WriteSignalLastSetup = DateTime.UtcNow;
        _ready2WriteSignalLast = DateTime.UtcNow.Subtract(_ready2WriteSignalStart).Ticks;

        if(_ready2WriteSignalMax < _ready2WriteSignalLast)
        {
            _ready2WriteSignalMax = _ready2WriteSignalLast;
            _ready2WriteSignalMaxSetup = DateTime.UtcNow;
        }
    }

    /*internal void Writing(int quantity)
    {
        _writingTimes++;
        _writing += (ulong)quantity;
        if(quantity > _writingMax)
            _writingMax = quantity;
    }


    internal void Reading(int quantity)
    {
        _readingTimes++;
        _reading += (ulong)quantity;
        if(quantity > _readingMax)
            _readingMax = quantity;
    }*/

    internal void TotalBytesInQueueError()
    {
        _errorTotalBytesInQueue++;
    }

    internal void Timeout()
    {
        _timeouts++;
        _timeoutsLastSetup = DateTime.UtcNow;
    }

    internal string Report()
    {
        var sb = new StringBuilder();
        sb.Append(CultureInfo.CurrentUICulture, $"Time: {DateTime.UtcNow:dd.MM.yyyy HH:mm:ss.ms}; Protocol: {Ipc?.ProtocolVersion}");
        sb.Append("<hr>");

        sb.Append(CultureInfo.CurrentUICulture, $"_ready2writeSignal_Calls: {_ready2WriteSignalCalls};");
        sb.Append("<br>");
        sb.Append(CultureInfo.CurrentUICulture, $"_ready2writeSignal_Max: {_ready2WriteSignalMax} ({_ready2WriteSignalMax / TimeSpan.TicksPerMillisecond}); Setup: {_ready2WriteSignalMaxSetup:dd.MM.yyyy HH:mm:ss.ms};");
        sb.Append("<br>");
        sb.Append(CultureInfo.CurrentUICulture, $"_ready2writeSignal_Last (shows when writer's await was set): {_ready2WriteSignalLast} ({_ready2WriteSignalLast / TimeSpan.TicksPerMillisecond}); Setup: {Ready2WriteSignalLastSetup:dd.MM.yyyy HH:mm:ss.ms}");
        sb.Append("<br>");
        sb.Append(CultureInfo.CurrentUICulture, $"_ready2ReadSignal_Last_Setup (shows when read's await was set): {Ready2ReadSignalLastSetup:dd.MM.yyyy HH:mm:ss.ms}");
        sb.Append("<br>");

        sb.Append("<hr>");
        sb.Append(CultureInfo.CurrentUICulture, $"_waitForRead_Max: {_waitForReadMax} ({_waitForReadMax / TimeSpan.TicksPerMillisecond}); Setup: {_waitForReadMaxSetup:dd.MM.yyyy HH:mm:ss.ms}");
        sb.Append("<br>");
        sb.Append(CultureInfo.CurrentUICulture, $"_readProcedure_Max: {_readProcedureMax} ({_readProcedureMax / TimeSpan.TicksPerMillisecond}); Setup: {_readProcedureMaxSetup:dd.MM.yyyy HH:mm:ss.ms}");
        sb.Append("<br>");


        sb.Append("<hr>");
        sb.Append(CultureInfo.CurrentUICulture, $"_writing: {_writing} bytes; Max: {_writingMax} bytes; Times: {_writingTimes}; Middle: {_writing / _writingTimes} bytes");
        sb.Append("<br>");
        sb.Append(CultureInfo.CurrentUICulture, $"_reading: {_reading} bytes; Max: {_readingMax} bytes; Times: {_readingTimes}; Middle: {_reading / _readingTimes} bytes");
        sb.Append("<br>");

        sb.Append("<hr>");
        sb.Append(CultureInfo.CurrentUICulture, $"TotalBytesInQueue: {TotalBytesInQueue};");
        sb.Append("<br>");
        sb.Append(CultureInfo.CurrentUICulture, $"_error_totalBytesInQueue: {_errorTotalBytesInQueue};");
        sb.Append("<br>");

        sb.Append("<hr>");
        sb.Append(CultureInfo.CurrentUICulture, $"_timeouts: {_timeouts}; Last setup: {_timeoutsLastSetup:dd.MM.yyyy HH:mm:ss.ms}");
        sb.Append("<br>");

        return sb.ToString();
    }
}
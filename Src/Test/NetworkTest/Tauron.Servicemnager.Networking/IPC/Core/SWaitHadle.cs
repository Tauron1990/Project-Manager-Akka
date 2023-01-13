namespace Tauron.Servicemnager.Networking.IPC.Core;

internal class SWaitHadle : EventWaitHandle
{
    internal SWaitHadle(bool initialState, EventResetMode mode, string name) : base(initialState, mode, name) { }
    //Global prefix and extran permissions
    //http://stackoverflow.com/questions/2590334/creating-a-cross-process-eventwaithandle
}
namespace Tauron.Application.AkkNode.Services.CommandsOld
{
    public interface ISender
    {
        void SendCommand(IReporterMessage command);
    }
}
namespace Tauron.Application.AkkaNode.Services.Reporting
{
    public interface IDelegatingMessage
    {
        Reporter Reporter { get; }

        string Info { get; }
    }
}
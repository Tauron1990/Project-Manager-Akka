namespace Tauron.Application.AkkaNode.Services
{
    public interface IDelegatingMessage
    {
        Reporter Reporter { get; }

        string Info { get; }
    }
}
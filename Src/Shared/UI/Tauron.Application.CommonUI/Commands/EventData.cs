namespace Tauron.Application.CommonUI.Commands;

public sealed class EventData
{
    public EventData(object sender, object eventArgs)
    {
        Sender = sender;
        EventArgs = eventArgs;
    }

    public object EventArgs { get; }

    public object Sender { get; }
}
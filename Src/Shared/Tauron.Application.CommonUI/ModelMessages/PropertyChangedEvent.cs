namespace Tauron.Application.CommonUI.ModelMessages
{
    public sealed record PropertyChangedEvent(string Name, object? Value);
}
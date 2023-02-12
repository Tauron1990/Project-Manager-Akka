namespace ServiceManager.Shared.Api
{
    public interface IApiMessageTranslator
    {
        string Translate<T>(T message);
    }
}
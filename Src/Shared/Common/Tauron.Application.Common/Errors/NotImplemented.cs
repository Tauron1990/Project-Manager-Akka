namespace Tauron.Errors;

public sealed class NotImplemented : Error
{
    public string Name { get; }

    public NotImplemented(string name)
    {
        Name = name;
        Message = $"The Function {name} is not Implemented";

        WithMetadata(nameof(Name), name);
    }
}
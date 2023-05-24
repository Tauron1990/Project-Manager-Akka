namespace Tauron.Errors;

public sealed class NullOrEmpty : Error
{
    public string Name { get; }

    public NullOrEmpty(string name)
    {
        Name = name;
        Message = $"Value {name} is null or Empty";

        WithMetadata(nameof(Name), name);
    }
}
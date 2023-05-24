namespace Tauron.Application.VirtualFiles;

public sealed class SchemeMismatch : Error
{
    public PathInfo Info { get; }
    public string Scheme { get; }

    public SchemeMismatch(PathInfo info, string scheme)
    {
        Info = info;
        Scheme = scheme;

        Message = $"Path {Info} has a Diffrent Scheme from {scheme}";

        WithMetadata(nameof(Info), info);
        WithMetadata(nameof(Scheme), scheme);
    }
}
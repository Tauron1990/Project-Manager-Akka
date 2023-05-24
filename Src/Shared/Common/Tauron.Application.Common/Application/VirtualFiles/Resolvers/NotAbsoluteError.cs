namespace Tauron.Application.VirtualFiles.Resolvers;

public sealed class NotAbsoluteError : Error
{
    public PathInfo Path { get; }

    public NotAbsoluteError(PathInfo path)
    {
        Path = path;
        Message = "No Absolut Path";
        
        WithMetadata(nameof(Path), path);
    }
}
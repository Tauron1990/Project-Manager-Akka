using Tauron.Errors;

namespace Tauron.Application.VirtualFiles.Resolvers;

public sealed class RsolverNotMatch : NotFoundError
{
    public PathInfo Path { get; }

    public RsolverNotMatch(PathInfo path)
    {
        Path = path;

        Message = "No Rsolver for Schemen Found";

        WithMetadata(nameof(Path), path);
    }
}
using Tauron.Errors;

namespace Tauron.Application.VirtualFiles;

public sealed class NoParentDirectory : NotFoundError
{
    public NoParentDirectory()
    {
        Message = "No Parent Directory";
    }
}
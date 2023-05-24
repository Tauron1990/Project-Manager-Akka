namespace Tauron.Application.VirtualFiles;

public sealed class NoFileName : Error
{
    public NoFileName()
    {
        Message = "No File Name Found";
    }
}
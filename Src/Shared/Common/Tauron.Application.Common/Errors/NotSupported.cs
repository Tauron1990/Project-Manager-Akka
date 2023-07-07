namespace Tauron.Errors;

public class NotSupported : Error
{
    public NotSupported()
    {
        Message = "Operation not Supported";
    }
}
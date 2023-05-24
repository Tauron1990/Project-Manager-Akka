namespace Tauron.Errors;

public sealed class InvalidOperation : Error
{
    public InvalidOperation()
    {
        Message = "The Operation is Invalid";
    }
}
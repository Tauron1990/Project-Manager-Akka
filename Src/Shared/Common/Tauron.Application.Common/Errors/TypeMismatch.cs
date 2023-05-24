namespace Tauron.Errors;

public sealed class TypeMismatch : Error
{
    public TypeMismatch()
    {
        Message = "The Requestet Type des not march the Provided";
    }
}
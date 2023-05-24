namespace Tauron.Errors;

public sealed class ConditionNotMet : Error
{
    public ConditionNotMet()
    {
        Message = "Condition not met";
    }
}
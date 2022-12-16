namespace Tauron.TextAdventure.Engine.Core;

public static class DelegateExtensions
{
    public static TDelegate Combine<TDelegate>(this TDelegate? delegate1, TDelegate delegate2)
        where TDelegate : Delegate
    {
        if(delegate1 is null) return delegate2;

        return (TDelegate)Delegate.Combine(delegate1, delegate2);
    }
}
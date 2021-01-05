namespace Tauron.Application.CommonUI
{
    public interface IUIObject
    {
        IUIObject? GetPerent();
        object Object { get; }
    }
}
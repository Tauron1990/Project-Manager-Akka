namespace Tauron.Application.CommonUI;

public interface IUIObject
{
    object Object { get; }
    IUIObject? GetPerent();
}
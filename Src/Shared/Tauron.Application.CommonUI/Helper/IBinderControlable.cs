namespace Tauron.Application.CommonUI.Helper
{
    public interface IBinderControllable
    {
        void Register(string key, IControlBindable bindable, IUIObject affectedPart);
        void CleanUp(string key);
    }
}
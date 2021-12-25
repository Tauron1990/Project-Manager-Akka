using System.Threading.Tasks;

namespace Tauron.Application.CommonUI;

public interface IWindow : IUIElement
{
    void Show();
    void Hide();
    Task<bool?> ShowDialog();
}
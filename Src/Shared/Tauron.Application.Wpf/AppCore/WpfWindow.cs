using System.Threading.Tasks;
using Tauron.Application.CommonUI;

namespace Tauron.Application.Wpf.AppCore
{
    public sealed class WpfWindow : WpfElement, IWindow
    {
        private readonly System.Windows.Window _window;

        public WpfWindow(System.Windows.Window window)
            : base(window) => _window = window;

        public System.Windows.Window Window => _window;
        public void Show() => _window.Show();
        public void Hide() => _window.Hide();
        public Task<bool?> ShowDialog() => _window.Dispatcher.InvokeAsync(() => _window.ShowDialog()).Task;
    }
}
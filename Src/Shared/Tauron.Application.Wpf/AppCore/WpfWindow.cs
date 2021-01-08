using System.Threading.Tasks;
using Tauron.Application.CommonUI;

namespace Tauron.Application.Wpf.AppCore
{
    public sealed class WpfWindow : WpfElement, IWindow
    {
        public WpfWindow(System.Windows.Window window)
            : base(window)
            => Window = window;

        public System.Windows.Window Window { get; }

        public void Show()
        {
            Window.Show();
        }

        public void Hide()
        {
            Window.Hide();
        }

        public Task<bool?> ShowDialog()
        {
            return Window.Dispatcher.InvokeAsync(() => Window.ShowDialog()).Task;
        }
    }
}
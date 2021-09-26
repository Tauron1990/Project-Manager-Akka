using System;

namespace WpfApp
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void MainWindow_OnClosed(object? sender, EventArgs e) => ((MainWindowModel) DataContext).Dispose();
    }
}
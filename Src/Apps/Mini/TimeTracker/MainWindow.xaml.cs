using System;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using MaterialDesignThemes.Wpf;
using Tauron.Application.CommonUI;
using Tauron.Application.CommonUI.AppCore;
using Tauron.Application.CommonUI.Dialogs;
using TimeTracker.Data;
using TimeTracker.ViewModels;

namespace TimeTracker
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : IMainWindow
    {
        public MainWindow(IViewModel<MainWindowViewModel> model, IDialogCoordinator dialogCoordinator)
            : base(model)
        {
            InitializeComponent();


            var evts = (IDialogCoordinatorUIEvents)dialogCoordinator;

            evts.HideDialogEvent += () => Dialogs.CurrentSession?.Close();
            evts.ShowDialogEvent += o => this.ShowDialog(o);

            Closed += (_, args) => Shutdown?.Invoke(this, args);
        }

        public event EventHandler? Shutdown;

        private void ModiferParameterClick(object sender, RoutedEventArgs e)
        {
            if (sender is ButtonBase button)
                button.CommandParameter = new ModiferBox(Keyboard.Modifiers);
        }
    }
}
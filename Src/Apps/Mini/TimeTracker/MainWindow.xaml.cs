﻿using System;
using MaterialDesignThemes.Wpf;
using Tauron.Application.CommonUI;
using Tauron.Application.CommonUI.AppCore;
using Tauron.Application.CommonUI.Dialogs;
using TimeTracker.ViewModels;

namespace TimeTracker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
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

            Closed += (sender, args) => Shutdown?.Invoke(sender, args);
        }

        public event EventHandler? Shutdown;
    }
}

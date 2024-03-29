﻿using System;
using System.Windows;
using Akka.MGIHelper.Core.Configuration;
using Tauron.Application.CommonUI;
using Tauron.Application.CommonUI.AppCore;

namespace Akka.MGIHelper
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : IMainWindow
    {
        private readonly IDisposable _setBlocker;
        private readonly WindowOptions _windowOptions;

        public MainWindow(IViewModel<MainWindowViewModel> model, WindowOptions windowOptions)
            : base(model)
        {
            _setBlocker = windowOptions.BlockSet();

            _windowOptions = windowOptions;
            InitializeComponent();

            WindowStartupLocation = WindowStartupLocation.Manual;
        }

        public event EventHandler? Shutdown;

        private void MainWindow_OnClosed(object? _, EventArgs e) => Shutdown?.Invoke(this, e);

        private void MainWindow_OnLocationChanged(object? sender, EventArgs e)
        {
            _windowOptions.PositionY = Top;
            _windowOptions.PositionX = Left;
        }

        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            Left = _windowOptions.PositionX;
            Top = _windowOptions.PositionY;
            _setBlocker.Dispose();
        }
    }
}
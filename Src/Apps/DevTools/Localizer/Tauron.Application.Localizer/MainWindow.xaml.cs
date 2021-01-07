using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.IsolatedStorage;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Xml.Serialization;
using Akka.Actor;
using AvalonDock;
using AvalonDock.Layout;
using MaterialDesignThemes.Wpf;
using Serilog;
using Tauron.Application.CommonUI;
using Tauron.Application.CommonUI.AppCore;
using Tauron.Application.CommonUI.Dialogs;
using Tauron.Application.Localizer.DataModel.Processing;
using Tauron.Application.Localizer.DataModel.Workspace;
using Tauron.Application.Localizer.UIModels;
using Tauron.Application.Localizer.UIModels.lang;
using Tauron.Application.Localizer.UIModels.Services;
using Tauron.Application.Localizer.Views.DockingPanes;

namespace Tauron.Application.Localizer
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : IMainWindow
    {
        private readonly IDialogCoordinator _coordinator;
        private readonly LocLocalizer _localizer;
        private readonly IMainWindowCoordinator _mainWindowCoordinator;
        private readonly ProjectFileWorkspace _workspace;
        private readonly CommonUIFramework _framework;

        public MainWindow(IViewModel<MainWindowViewModel> model, LocLocalizer localizer, IMainWindowCoordinator mainWindowCoordinator, IDialogCoordinator coordinator, ProjectFileWorkspace workspace,
            CommonUIFramework framework, IOperationManager operationManager)
            : base(model)
        {
            _localizer = localizer;
            _mainWindowCoordinator = mainWindowCoordinator;
            _coordinator = coordinator;
            _workspace = workspace;
            _framework = framework;

            InitializeComponent();

            var diag = ((IDialogCoordinatorUIEvents) _coordinator);

            diag.ShowDialogEvent += o => this.ShowDialog(o);
            diag.HideDialogEvent += () => Dialogs.CurrentSession?.Close();

            _mainWindowCoordinator.TitleChanged += () => Dispatcher.BeginInvoke(new Action(MainWindowCoordinatorOnTitleChanged));
            _mainWindowCoordinator.IsBusyChanged += IsBusyChanged;

            Closing += OnClosing;
            Closed += async (_, _) =>
            {
                SaveLayout();
                Shutdown?.Invoke(this, EventArgs.Empty);

                await Task.Delay(TimeSpan.FromSeconds(60));
                Process.GetCurrentProcess().Kill(false);
            };

            operationManager.OperationFailed
                .ObserveOnDispatcher()
                .Subscribe(f =>
                {
                    if (Snackbar.MessageQueue == null)
                        Snackbar.MessageQueue = new SnackbarMessageQueue(TimeSpan.FromSeconds(5));
                    Snackbar.MessageQueue.Enqueue($"{f.Status ?? " / "}");
                });
        }
        
        public event EventHandler? Shutdown;

        private void IsBusyChanged() => Dispatcher.Invoke(() => BusyIndicator.IsBusy = _mainWindowCoordinator.IsBusy);

        private bool _forceClose = false;

        private void OnClosing(object sender, CancelEventArgs e)
        {
            if(_forceClose) return;
            if (_mainWindowCoordinator.Saved) return;

            e.Cancel = true;

            if (Dialogs.IsOpen)
                Dialogs.IsOpen = false;

            Dialogs.ShowDialog(_framework.CreateDefaultMessageContent(_localizer.CommonWarnig, _localizer.MainWindowCloseWarning,
                    res => Dialogs.CurrentSession.Close(res == true), true))
                .ContinueWith(t =>
                {
                    if (!t.IsCompletedSuccessfully || t.Result is not bool result) return;
                    if (!result) return;

                    if (!_workspace.ProjectFile.IsEmpty)
                        _workspace.ProjectFile.Operator.Tell(ForceSave.Seal(_workspace.ProjectFile), ActorRefs.NoSender);

                    _forceClose = true;
                    Dispatcher.InvokeAsync(Close);
                });
        }

        private void MainWindowCoordinatorOnTitleChanged() => Title = _localizer.MainWindowTitle + " - " + _mainWindowCoordinator.TitlePostfix;

        private const string LayoutFile = "Layout.xml";

        private void SaveLayout()
        {
            using var stream = IsolatedStorageFile.GetUserStoreForApplication().OpenFile(LayoutFile, FileMode.Create);
            var ser = new XmlSerializer(typeof(LayoutRoot));
            ser.Serialize(stream, DockingManager.Layout);
        }

        private void FrameworkElement_OnLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                var store = IsolatedStorageFile.GetUserStoreForApplication();
                if (store.FileExists(LayoutFile))
                {
                    var seralizer = new XmlSerializer(typeof(LayoutRoot));
                    using var stream = store.OpenFile(LayoutFile, FileMode.Open);
                    if(!(seralizer.Deserialize(stream) is LayoutRoot layout))
                        DockReset(null, null);
                    else
                        DockingManager.Layout = layout;
                }
                else
                    DockReset(null, null);
            }
            catch (Exception exception)
            {
                Log.ForContext<MainWindow>().Error(exception, "Error on Load Dock State");
                DockReset(null, null);
            }
        }

        private void DockingManager_OnLayoutChanging(object? sender, EventArgs e)
        {
            var layout = ((DockingManager) sender!).Layout;
            LayoutBuilder.ProcessLayout(layout);
        }

        private void DockReset(object? sender, RoutedEventArgs? e)
        {
            DockingManager.Layout = (LayoutRoot) FindResource("LayoutRoot");
            DockingManager.UpdateLayout();
        }
    }
}
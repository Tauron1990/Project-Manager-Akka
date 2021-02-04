using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using JetBrains.Annotations;
using Servicemnager.Networking.IPC;
using Tauron;

namespace WpfApp
{
    public class MainWindowModel : INotifyPropertyChanged, IDisposable
    {
        public const string Id = "697E36BC-028B-46C3-A253-AC3374FA4652";

        private readonly Dispatcher _dispatcher = Application.Current.Dispatcher;
        private readonly IpcConnection _ipc;
        private string _toSend = string.Empty;

        public event PropertyChangedEventHandler? PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public SimpleCommand NewProcess { get; }

        public SimpleCommand Send { get; }

        public string ToSend
        {
            get => _toSend;
            set
            {
                if (value == _toSend) return;
                _toSend = value;
                OnPropertyChanged();
            }
        }
        
        public string Mode { get; }

        public ObservableCollection<string> Input { get; }

        public ObservableCollection<string> Output { get; }

        public MainWindowModel()
        {
            var master = SharmComunicator.MasterIpcReady(Id);
            Mode = !master ? "Server" : "Client";

            _ipc = new IpcConnection(master, master ? IpcApplicationType.Client : IpcApplicationType.Server, ErrorHandler);
            _ipc.Start();
            var handler = _ipc.OnMessage<TestMessage>();

            handler.OnError().Subscribe(e => ErrorHandler("Serialization", e));
            handler.OnResult().Subscribe(InputHandler);

            Input = new ObservableCollection<string>();
            Output = new ObservableCollection<string>();
            NewProcess = new SimpleCommand(StartNew);
            Send = new SimpleCommand(() => Task.Run(SendData));
        }

        private void StartNew()
        {
            Log("New Process");

//#if DEBUG
//            new MainWindow().Show();
//#else
            Process.Start("WpfApp.exe");
//#endif
        }

        private void SendData()
        {
            try
            {
                Log("Send Data");
                Log(_ipc.SendMessage(new TestMessage(ToSend)) ? "Send Compled" : "Send Failed");
            }
            catch (Exception e)
            {
                ErrorHandler("Internal", e);
            }
        }

        private void ErrorHandler(string arg1, Exception arg2)
        {
            string msg = $"Error {Environment.NewLine} {arg1} {Environment.NewLine} {arg2}";

            _dispatcher.Invoke(() => Output.Add(msg));
        }

        private void InputHandler(TestMessage testMsg)
        {
            string msg = $"Message: {testMsg.Message}";

            _dispatcher.Invoke(() => Input.Add(msg));
        }

        private void Log(string info)
            => _dispatcher.Invoke(() => Output.Add(info));

        public void Dispose() => _ipc?.Dispose();
    }
}
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using tiesky.com;
using WpfApp.Annotations;

namespace WpfApp
{
    public class MainWindowModel : INotifyPropertyChanged, IDisposable
    {
        private static string _id = "697E36BC-028B-46C3-A253-AC3374FA4652";

        private readonly Dispatcher _dispatcher = Application.Current.Dispatcher;
        private readonly SharmIpc _ipc;
        private string _toSend;

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

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

        public ObservableCollection<string> Input { get; }

        public ObservableCollection<string> Output { get; }

        public MainWindowModel()
        {
            _ipc = new SharmIpc(_id, InputHandler, ExternalExceptionHandler:ErrorHandler, protocolVersion:SharmIpc.eProtocolVersion.V2);
            Input = new ObservableCollection<string>();
            Output = new ObservableCollection<string>();
            NewProcess = new SimpleCommand(StartNew);
            Send = new SimpleCommand(() => Task.Run(SendData));
        }

        private void StartNew()
        {
            Log("New Process");

            Process.Start("WpfApp.exe");
        }

        private void SendData()
        {
            try
            {
                Log("Send Data");
                if (_ipc.RemoteRequestWithoutResponse(Encoding.UTF8.GetBytes(ToSend))) 
                    Log("Send Compled");
                else
                    Log("Send Failed");
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

        private void InputHandler(ulong arg1, byte[] arg2)
        {
            string msg = $"Id: {arg1} -- Message: {Encoding.UTF8.GetString(arg2)}";

            _dispatcher.Invoke(() => Input.Add(msg));
        }

        private void Log(string info)
            => _dispatcher.Invoke(() => Output.Add(info));

        public void Dispose() => _ipc?.Dispose();
    }
}
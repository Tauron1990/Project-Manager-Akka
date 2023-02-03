using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Tauron.Application.Localizer.UIModels.lang;
using Tauron.Application.Localizer.UIModels.Views;

namespace Tauron.Application.Localizer.Views
{
    /// <summary>
    ///     Interaktionslogik für OpenFileDialogView.xaml
    /// </summary>
    public partial class OpenFileDialogView : IOpenFileDialog
    {
        private readonly IDialogFactory _dialogFactory;
        private readonly OpenFileMode _filemode;
        private readonly LocLocalizer _localizer;
        private readonly TaskCompletionSource<string?> _selector = new();

        public OpenFileDialogView(IDialogFactory dialogFactory, LocLocalizer localizer, OpenFileMode filemode)
        {
            _dialogFactory = dialogFactory;
            _localizer = localizer;
            _filemode = filemode;
            InitializeComponent();

            if (filemode == OpenFileMode.OpenNewFile)
                Title.Text += _localizer.OpenFileDialogViewHeaderNewPrefix;
        }

        public Task<string?> Init(Unit initalData) => _selector.Task;

        private void Search_OnClick(object sender, RoutedEventArgs e)
        {
            var dis = Disposable.Empty;

            dis = _dialogFactory.ShowOpenFileDialog(
                    null,
                    _filemode == OpenFileMode.OpenExistingFile,
                    "transp",
                    dereferenceLinks: true,
                    _localizer.OpenFileDialogViewDialogFilter,
                    multiSelect: false,
                    _localizer.OpenFileDialogViewDialogTitle,
                    validateNames: true,
                    checkPathExists: true)
               .NotNull()
               .Select(s => s.FirstOrDefault())
               .NotNull()
               .ObserveOnDispatcher()
               .Subscribe(
                    s =>
                    {
                        PART_Path.Text = s;
                        // ReSharper disable once AccessToModifiedClosure
                        dis.Dispose();
                    });
        }

        private void Ready_OnClick(object sender, RoutedEventArgs e)
        {
            IsEnabled = false;
            string text = PART_Path.Text;
            _selector.SetResult(text);
        }

        private void OpenFileDialogView_OnLoaded(object sender, RoutedEventArgs e)
        {
            FocusManager.SetFocusedElement(this, PART_Path);
            Keyboard.Focus(PART_Path);
        }
    }
}
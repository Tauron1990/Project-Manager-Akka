using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using JetBrains.Annotations;

namespace Tauron.Application.Avalonia.Dialogs
{
    public class MessageDialog : DialogBase
    {
        private Button CancelButton => this.Get<Button>("CancelButton");
        private TextBlock ContentBox => this.Get<TextBlock>("ContentBox");

        private readonly bool _canCnacel;
        private readonly Action<bool?>? _result;

        public MessageDialog()
        {
            throw new NotSupportedException(); // Compiler Enforcement
            //InitializeComponent();

        }
        
        public MessageDialog(string title, string content, Action<bool?>? result, bool canCnacel)
        {
            _result = result;
            _canCnacel = canCnacel;
            InitializeComponent();
            Title = title;
            ContentBox.Text = content;

            if (!canCnacel)
                CancelButton.IsVisible = false;
        }

        [UsedImplicitly]
        private void Ok_OnClick(object sender, RoutedEventArgs e)
        {
            if (_canCnacel)
                _result?.Invoke(true);
            else
                _result?.Invoke(null);
        }

        [UsedImplicitly]
        private void Cancel_OnClick(object sender, RoutedEventArgs e)
        {
            _result?.Invoke(false);
        }
        
        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using MaterialDesignExtensions.Model;
using Tauron.Application.CommonUI.Commands;
using Tauron.Application.Localizer.UIModels.lang;
using Tauron.Application.Localizer.UIModels.Views;

namespace Tauron.Application.Localizer.Views
{
    /// <summary>
    ///     Interaktionslogik für NewEntryDialogView.xaml
    /// </summary>
    public partial class NewEntryDialogView : INewEntryDialog
    {
        private readonly LocLocalizer _localizer;

        public NewEntryDialogView(LocLocalizer localizer)
        {
            _localizer = localizer;
            InitializeComponent();
        }

        public Task<NewEntryDialogResult?> Init(IEnumerable<NewEntryInfoBase> initalData)
        {
            var result = new TaskCompletionSource<NewEntryDialogResult?>();

            DataContext = new NewEntryDialogViewModel(
                s => result.SetResult(string.IsNullOrWhiteSpace(s) ? null : new NewEntryDialogResult(s)), initalData,
                _localizer);

            return result.Task;
        }

        private void NewEntryDialogView_OnLoaded(object sender, RoutedEventArgs e)
        {
            FocusManager.SetFocusedElement(this, NameBox);
            Keyboard.Focus(NameBox);
        }
    }

    public sealed class NewEntryDialogViewModel : ObservableObject
    {
        private readonly string[] _entrys;
        private readonly LocLocalizer _localizer;

        private string _content = string.Empty;
        private string _error = string.Empty;

        public NewEntryDialogViewModel(Action<string?> selector, IEnumerable<NewEntryInfoBase> infos,
            LocLocalizer localizer)
        {
            _localizer = localizer;

            List<string> entrys = new();
            List<string> suggests = new();

            foreach (var info in infos)
                switch (info)
                {
                    case NewEntryInfo entry:
                        entrys.Add(entry.Name);
                        break;
                    case NewEntrySuggestInfo suggest:
                        suggests.Add(suggest.Name);
                        break;
                }

            _entrys = entrys.ToArray();
            Suggests = new StaticSuggestion(suggests);

            Return = new SimpleCommand(() => string.IsNullOrWhiteSpace(Error), () => selector(Content));
        }

        public ITextBoxSuggestionsSource Suggests { get; }

        public string Content
        {
            get => _content;
            set
            {
                if (value == _content) return;
                _content = value;
                OnPropertyChanged();
                Error = _entrys.Contains(value) ? _localizer.NewEntryDialogViewDuplicateError : ValidateValue(value);
            }
        }

        public string Error
        {
            get => _error;
            set
            {
                if (value == _error) return;
                _error = value;
                OnPropertyChanged();
            }
        }

        public ICommand Return { get; }

        private string ValidateValue(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return string.Empty;

            if (char.IsDigit(value[0]))
                return string.Format(_localizer.NewEntryDialogViewCharError, 0);

            for (var index = 0; index < value.Length; index++)
            {
                var c = value[index];
                if (char.IsLetter(c) || char.IsDigit(c) || c == '_' || c == '-') continue;

                return string.Format(_localizer.NewEntryDialogViewCharError, index + 1);
            }

            return string.Empty;
        }

        private sealed class StaticSuggestion : TextBoxSuggestionsSource
        {
            private readonly List<string> _infos;

            public StaticSuggestion(List<string> infos) => _infos = infos;

            public override IEnumerable<string> Search(string searchTerm)
            {
                return _infos.Where(s => s.Contains(searchTerm));
            }
        }
    }
}
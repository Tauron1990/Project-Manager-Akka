﻿using System;
using Tauron.Application.Localizer.DataModel;

namespace Tauron.Application.Localizer.UIModels
{
    public sealed class ProjectLangEntry : ObservableObject
    {
        private readonly Action<string, ActiveLanguage> _changed;
        private string _content;

        public ProjectLangEntry(Action<string, ActiveLanguage> changed, ActiveLanguage language, string content)
        {
            _changed = changed;
            _content = content;
            Language = language;
        }

        public ActiveLanguage Language { get; }

        public string Content
        {
            get => _content;
            set
            {
                if (value == _content) return;
                _content = value;
                OnPropertyChanged();
                _changed(value, Language);
            }
        }

        public void UpdateContent(string content)
        {
            _content = content;
            OnPropertyChangedExplicit(nameof(Content));
        }
    }
}
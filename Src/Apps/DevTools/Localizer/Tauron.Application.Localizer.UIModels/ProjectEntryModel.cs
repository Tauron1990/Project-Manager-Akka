using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using DynamicData;
using JetBrains.Annotations;
using Tauron.Application.CommonUI;
using Tauron.Application.CommonUI.Commands;
using Tauron.Application.Localizer.DataModel;

namespace Tauron.Application.Localizer.UIModels
{
    [PublicAPI]
    public sealed class ProjectEntryModel : ObservableObject, IDisposable
    {
        private readonly IDisposable _connection;
        private readonly SourceList<ProjectLangEntry> _entries = new();
        private readonly string _projectName;
        private readonly Action<(string ProjectName, string EntryName, ActiveLanguage Lang, string Content)> _updater;

        public ProjectEntryModel(
            Project project, LocEntry target,
            Action<(string ProjectName, string EntryName, ActiveLanguage Lang, string Content)> updater,
            Action<(string ProjectName, string EntryName)> remove)
        {
            _updater = updater;
            _projectName = project.ProjectName;
            EntryName = target.Key;

            _connection = _entries.Connect().ObserveOnDispatcher().Bind(out var entrys).Subscribe();
            Entries = entrys;

            RemoveCommand = new SimpleCommand(() => remove((_projectName, EntryName)));
            CopyCommand = new SimpleCommand(() => Clipboard.SetText(EntryName));

            foreach (var language in project.ActiveLanguages)
                _entries.Add(
                    target.Values.TryGetValue(language, out var content)
                        ? new ProjectLangEntry(EntryChanged, language, content)
                        : new ProjectLangEntry(EntryChanged, language, string.Empty));
        }

        public string EntryName { get; }

        public ReadOnlyObservableCollection<ProjectLangEntry> Entries { get; }

        public ICommand CopyCommand { get; }

        public ICommand RemoveCommand { get; }

        public void Dispose()
        {
            _entries.Dispose();
            _connection.Dispose();
        }

        private void EntryChanged(string content, ActiveLanguage language)
        {
            _updater((_projectName, EntryName, language, content));
        }

        public void AddLanguage(ActiveLanguage lang)
        {
            _entries.Add(new ProjectLangEntry(EntryChanged, lang, string.Empty));
        }

        public void Update(LocEntry entry)
        {
            foreach (var (activeLanguage, content) in entry.Values)
            {
                var ent = Entries.FirstOrDefault(f => f.Language == activeLanguage);
                if (ent == null)
                    _entries.Add(new ProjectLangEntry(EntryChanged, activeLanguage, content));
                else
                    ent.UpdateContent(content);
            }
        }
    }
}
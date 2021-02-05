﻿using System.Collections.Immutable;
using System.Linq;
using System.Reactive.Linq;
using JetBrains.Annotations;
using Tauron.Application.Localizer.DataModel.Workspace.Mutating.Changes;
using Tauron.Application.Workshop.Mutating;
using Tauron.Application.Workshop.Mutation;

namespace Tauron.Application.Localizer.DataModel.Workspace.Mutating
{
    [PublicAPI]
    public class EntryMutator
    {
        private readonly MutatingEngine<MutatingContext<ProjectFile>> _engine;

        public EntryMutator(MutatingEngine<MutatingContext<ProjectFile>> engine)
        {
            _engine = engine;

            EntryRemove = engine.EventSource(context => new EntryRemove(context.GetChange<RemoveEntryChange>().Entry),
                context => context.Change is RemoveEntryChange);
            EntryUpdate = engine.EventSource(context => new EntryUpdate(context.GetChange<EntryChange>().Entry),
                context => context.Change is EntryChange);
            EntryAdd = engine.EventSource(context => context.GetChange<NewEntryChange>().ToData(),
                context => context.Change is NewEntryChange);
        }

        public IEventSource<EntryRemove> EntryRemove { get; }

        public IEventSource<EntryUpdate> EntryUpdate { get; }

        public IEventSource<EntryAdd> EntryAdd { get; }

        public void RemoveEntry(string project, string name)
        {
            _engine.Mutate(nameof(RemoveEntry),
                obs => obs.Select(context =>
                {
                    var entry = context.Data.Projects.FirstOrDefault(p => p.ProjectName == project)?.Entries
                        .Find(le => le.Key == name);
                    return entry == null
                        ? context
                        : context.Update(new RemoveEntryChange(entry), context.Data.ReplaceEntry(entry, null));
                }));
        }

        public void UpdateEntry(string project, ActiveLanguage lang, string name, string content)
        {
            _engine.Mutate(nameof(UpdateEntry),
                obs => obs.Select(context =>
                {
                    var entry = context.Data.Projects.FirstOrDefault(p => p.ProjectName == project)?.Entries
                        .Find(le => le.Key == name);

                    if (entry == null) return context;
                    var oldContent = entry.Values.GetValueOrDefault(lang);
                    var newEntry = entry with
                    {
                        Values = oldContent == null
                            ? entry.Values.Add(lang, content)
                            : entry.Values.SetItem(lang, content)
                    };

                    return context.Update(new EntryChange(newEntry), context.Data.ReplaceEntry(entry, newEntry));
                }));
        }

        public void NewEntry(string project, string name)
        {
            _engine.Mutate(nameof(NewEntry),
                obs => obs.Select(context =>
                {
                    var proj = context.Data.Projects.Find(p => p.ProjectName == project);
                    if (proj == null || proj.Entries.Any(l => l.Key == name)) return context;

                    var newEntry = new LocEntry(project, name);
                    var newData = context.Data.ReplaceEntry(null, newEntry);
                    return context.Update(new NewEntryChange(newEntry), newData);
                }));
        }
    }
}
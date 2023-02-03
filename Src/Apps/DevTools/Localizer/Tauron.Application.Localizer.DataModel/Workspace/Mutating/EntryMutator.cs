using System.Collections.Generic;
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

            EntryRemove = engine.EventSource(
                context => new EntryRemove(context.GetChange<RemoveEntryChange>().Entry),
                context => context.Change is RemoveEntryChange);
            EntryUpdate = engine.EventSource(
                context => new EntryUpdate(context.GetChange<EntryChange>().Entry),
                context => context.Change is EntryChange);
            EntryAdd = engine.EventSource(
                context => context.GetChange<NewEntryChange>().ToData(),
                context => context.Change is NewEntryChange);
        }

        public IEventSource<EntryRemove> EntryRemove { get; }

        public IEventSource<EntryUpdate> EntryUpdate { get; }

        public IEventSource<EntryAdd> EntryAdd { get; }

        public void RemoveEntry(string project, string name)
        {
            _engine.Mutate(
                nameof(RemoveEntry),
                obs => obs.Select(
                    context =>
                    {
                        LocEntry? entry = context.Data.Projects.FirstOrDefault(p => string.Equals(p.ProjectName, project, System.StringComparison.Ordinal))?.Entries
                           .Find(le => string.Equals(le.Key, name, System.StringComparison.Ordinal));

                        return entry is null
                            ? context
                            : context.Update(new RemoveEntryChange(entry), context.Data.ReplaceEntry(entry, newEntry: null));
                    }));
        }

        public void UpdateEntry(string project, ActiveLanguage lang, string name, string content)
        {
            _engine.Mutate(
                nameof(UpdateEntry),
                obs => obs.Select(
                    context =>
                    {
                        var entry = context.Data.Projects.FirstOrDefault(p => string.Equals(p.ProjectName, project, System.StringComparison.Ordinal))?.Entries
                           .Find(le => string.Equals(le.Key, name, System.StringComparison.Ordinal));

                        if (entry is null) return context;

                        var oldContent = entry.Values.GetValueOrDefault(lang);
                        var newEntry = entry with
                                       {
                                           Values = oldContent is null
                                               ? entry.Values.Add(lang, content)
                                               : entry.Values.SetItem(lang, content)
                                       };

                        return context.Update(new EntryChange(newEntry), context.Data.ReplaceEntry(entry, newEntry));
                    }));
        }

        public void NewEntry(string project, string name)
        {
            _engine.Mutate(
                nameof(NewEntry),
                obs => obs.Select(
                    context =>
                    {
                        var proj = context.Data.Projects.Find(p => string.Equals(p.ProjectName, project, System.StringComparison.Ordinal));

                        if (proj is null || proj.Entries.Any(l => string.Equals(l.Key, name, System.StringComparison.Ordinal)))
                            return context;

                        var newEntry = new LocEntry(project, name);
                        var newData = context.Data.ReplaceEntry(oldEntry: null, newEntry);

                        return context.Update(new NewEntryChange(newEntry), newData);
                    }));
        }
    }
}
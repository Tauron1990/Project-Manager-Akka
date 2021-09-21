using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using ServiceManager.Client.ViewModels.Configuration;
using Tauron.Application;

namespace ServiceManager.Client.Shared.Configuration.ConditionEditor
{
    public sealed class EditorState
    {
        public bool ChangesWhereMade { get; set; }

        public List<ElementItem> ActualItems { get; } = new();

        public bool CommitChanges(AppConfigModel toModel, IEventAggregator aggregator)
        {
            var validationResult = ActualItems
               .Select(e => e.Validate(single: false))
               .FirstOrDefault(s => !string.IsNullOrWhiteSpace(s));

            if (string.IsNullOrWhiteSpace(validationResult))
            {
                ChangesWhereMade = false;
                toModel.UpdateCondiditions = true;
                toModel.Conditions = ActualItems.Select(i => i.Build()).Where(e => e != null).ToImmutableList()!;

                return true;
            }

            aggregator.PublishWarnig($"Fehler bei der Validireung der Bedinungen: {Environment.NewLine} {validationResult}");

            return false;
        }

        public void Reset()
        {
            ChangesWhereMade = false;
            ActualItems.Clear();
        }
    }
}
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using ServiceManager.Client.ViewModels;

namespace ServiceManager.Client.Shared.Configuration.ConditionEditor
{
    public sealed class EditorState
    {
        public bool ChangesWhereMade { get; set; }
        
        public List<ElementItem> ActualItems { get; } = new();

        public void CommitChanges(AppConfigModel toModel)
        {
            ChangesWhereMade = false;
            toModel.UpdateCondiditions = true;
            toModel.Conditions = ActualItems.Select(i => i.Build()).Where(e => e != null).ToImmutableList()!;
        }

        public void Reset()
        {
            ChangesWhereMade = false;
            ActualItems.Clear();
        }
    }
}
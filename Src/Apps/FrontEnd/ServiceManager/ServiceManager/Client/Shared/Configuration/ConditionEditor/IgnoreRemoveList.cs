using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace ServiceManager.Client.Shared.Configuration.ConditionEditor
{
    public class IgnoreList : Collection<ElementItem>
    {
        public IgnoreList(IList<ElementItem> list)
            : base(list) { }

        protected override void ClearItems() { }

        protected override void RemoveItem(int index) { }

        protected override void SetItem(int index, ElementItem item) { }

        protected override void InsertItem(int index, ElementItem item) { }
    }
}
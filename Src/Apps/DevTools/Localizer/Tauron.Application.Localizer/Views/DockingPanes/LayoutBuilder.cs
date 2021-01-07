using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using AvalonDock.Layout;

namespace Tauron.Application.Localizer.Views.DockingPanes
{
    public class LayoutBuilder
    {
        private static ReadOnlyDictionary<string, Func<object>> _map => new(new Dictionary<string, Func<object>>
        {
            {"Operation", () => new OperationPane() },
            { "Logger", () => new LoggerPane() },
            {"Projects", () => new CenterViewPane() },
            {"BuildControl", () => new BuildPane() },
            {"Analyzer", () => new AnalyserPane() }
        });
        
        private static void TrySetContent(LayoutContent content)
        {
            if(content.Content != null) return;

            if (!_map.TryGetValue(content.ContentId, out var fac)) return;

            content.Content = fac();
        }

        public static void ProcessLayout(ILayoutElement element)
        {
            switch (element)
            {
                case ILayoutContainer container:
                    container.Children.Foreach(ProcessLayout);
                    break;
                case LayoutContent content:
                    TrySetContent(content);
                    break;
            }
        }
    }
}
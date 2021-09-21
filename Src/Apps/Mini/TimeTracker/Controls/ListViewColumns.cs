using System.Collections.Immutable;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using JetBrains.Annotations;

namespace TimeTracker.Controls
{
    /// ListViewColumnStretch
    [PublicAPI]
    public class ListViewColumns : DependencyObject
    {
        /// IsStretched Dependancy property which can be attached to gridview columns.
        public static readonly DependencyProperty StretchProperty =
            DependencyProperty.RegisterAttached(
                "Stretch",
                typeof(bool),
                typeof(ListViewColumns),
                new UIPropertyMetadata(true, null, OnCoerceStretch));

        public static readonly DependencyProperty CustomStrechProperty = DependencyProperty.RegisterAttached(
            "CustomStrech",
            typeof(bool),
            typeof(ListViewColumns),
            new PropertyMetadata(default(bool), ChangeWidth));

        public static readonly DependencyProperty CustomWithProperty = DependencyProperty.RegisterAttached(
            "CustomWidth",
            typeof(double),
            typeof(ListViewColumns),
            new PropertyMetadata(-1d, ChangeWidth));

        private static void ChangeWidth(DependencyObject o, DependencyPropertyChangedEventArgs _)
        {
            if (o is ListView lv && GetStretch(lv)) SetColumnWidths(lv);
        }

        public static void SetCustomWidth(DependencyObject element, double value) => element.SetValue(CustomWithProperty, value);

        public static double GetCustomWidth(DependencyObject element) => (double)element.GetValue(CustomWithProperty);

        public static void SetCustomStrech(DependencyObject element, bool value) => element.SetValue(CustomStrechProperty, value);

        public static bool GetCustomStrech(DependencyObject element) => (bool)element.GetValue(CustomStrechProperty);

        public static bool GetStretch(DependencyObject obj) => (bool)obj.GetValue(StretchProperty);

        public static void SetStretch(DependencyObject obj, bool value) => obj.SetValue(StretchProperty, value);


        private static object OnCoerceStretch(DependencyObject source, object value)
        {
            if (source is not ListView lv) return value;

            lv.Loaded += lv_Loaded;
            lv.SizeChanged += lv_SizeChanged;

            return value;
        }


        private static void lv_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (sender is ListView { IsLoaded: true } list)
                SetColumnWidths(list);
        }

        private static void lv_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is ListView lv)
                //Set our initial widths.
                SetColumnWidths(lv);
        }

        /// Sets the column widths.
        private static void SetColumnWidths(ListView listView)
        {
            if (!GetCustomStrech(listView))
            {
                if (listView.View is not GridView gridView) return;

                var actualWith = gridView.Columns.Skip(1).Aggregate(listView.ActualWidth, (current, column) => current - column.ActualWidth);

                if (actualWith < 1) return;

                gridView.Columns[0].Width = actualWith - 30;
            }
            else
            {
                if (listView.View is not GridView gridView) return;

                var elements = ImmutableArray<GridViewColumn>.Empty;
                var max = 30d;

                foreach (var gridViewColumn in gridView.Columns)
                {
                    var w = GetCustomWidth(gridViewColumn);
                    // ReSharper disable once CompareOfFloatsByEqualityOperator
                    if (w == -1)
                    {
                        elements = elements.Add(gridViewColumn);
                    }
                    else
                    {
                        gridViewColumn.Width = w;
                        max += w;
                    }
                }

                var set = (listView.ActualWidth - max) / elements.Length;

                if (set < 10) return;

                foreach (var ele in elements) ele.Width = set;
            }
        }
    }
}
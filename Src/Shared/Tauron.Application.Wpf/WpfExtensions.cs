using System;
using System.ComponentModel;
using System.Windows.Data;
using JetBrains.Annotations;
using Tauron.Application.CommonUI.Model;

namespace Tauron.Application.Wpf
{
    [PublicAPI]
    public static class WpfExtensions
    {
        public static FluentCollectionPropertyRegistration<TData> AndConfigurateView<TData>(
            this FluentCollectionPropertyRegistration<TData> collection, Action<ICollectionView> view)
        {
            view(CollectionViewSource.GetDefaultView(collection.Property.Value));

            return collection;
        }
    }
}
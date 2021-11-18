using System;
using System.Linq.Expressions;
using JetBrains.Annotations;
using ReactiveUI;

namespace Tauron.Application.Blazor;

[PublicAPI]
public static class ViewForExtensions
{

    public IReactiveBinding<TView, TVProp> BindCollection<TViewModel, TView, TVmProp, TVProp>(TViewModel? viewModel, TView view, Expression<Func<TViewModel, TVmProp?>> vmProperty, Expression<Func<TView, TVProp>> viewProperty, object? conversionHint = null, IBindingTypeConverter? vmToViewConverterOverride = null) 
        where TViewModel : class 
        where TView : class, IViewFor
    {
        view.
    }
}
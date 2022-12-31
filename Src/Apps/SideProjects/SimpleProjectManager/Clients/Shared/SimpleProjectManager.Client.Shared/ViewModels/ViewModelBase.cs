using System;
using System.Runtime.CompilerServices;
using ReactiveUI;
using Vogen;

namespace SimpleProjectManager.Client.Shared.ViewModels;

public abstract class ViewModelBase : ReactiveObject, IActivatableViewModel
{
    public ViewModelActivator Activator { get; } = new();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected TValue NotNull<TValue>(TValue? value, string name)
    {
        if(value is null) throw new InvalidOperationException($"The Value {name} is null");

        return value;
    }
}
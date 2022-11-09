using SimpleProjectManager.Client.Shared.ViewModels;
using Tauron.Application;

namespace SimpleProjectManager.Client.Avalonia.Models;

public sealed record Navigating(ViewModelBase Model);

public sealed class NavigatingEvent : AggregateEvent<Navigating> { }